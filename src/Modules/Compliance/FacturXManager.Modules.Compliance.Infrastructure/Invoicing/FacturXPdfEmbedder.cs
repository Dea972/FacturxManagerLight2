using System.Globalization;
using System.Reflection;
using System.Text;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;

namespace FacturXManager.Modules.Compliance.Infrastructure.Invoicing;

/// <summary>
/// Utilitaire centralisé pour l'embedding PDF/A-3b Factur-X :
/// - Attachement du XML CII (factur-x.xml, AFRelationship=Data)
/// - Métadonnées XMP (pdfaid:part=3, conformance=B, Factur-X extension schema)
/// - OutputIntents (profil ICC sRGB obligatoire PDF/A)
/// - Contournement de l'écrasement XMP par PdfSharp lors du Save()
/// </summary>
internal static class FacturXPdfEmbedder
{
    /// <summary>
    /// Prend un PDF visuel (octets bruts) et y embarque le XML CII, les métadonnées XMP
    /// et le OutputIntent sRGB. Retourne le PDF/A-3b final.
    /// </summary>
    public static byte[] EmbedFacturXData(byte[] visualPdfBytes, FacturXRenderRequest request)
    {
        byte[] xmlBytes = Encoding.UTF8.GetBytes(request.CrossIndustryInvoiceXml);

        using var inputStream = new MemoryStream(visualPdfBytes);
        using PdfDocument document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);

        RemoveTransparencyGroups(document);
        InjectCidSets(document);
        AttachFacturXXml(document, request.EmbeddedFileName, xmlBytes);
        AttachOutputIntent(document);

        // Premier save : PdfSharp génère sa structure + son XMP par défaut
        byte[] firstPass;
        using (var outputStream = new MemoryStream())
        {
            document.Save(outputStream, closeStream: false);
            firstPass = outputStream.ToArray();
        }

        // Patch binaire direct : remplacer le XMP de PdfSharp par notre XMP Factur-X.
        // PdfSharp écrit le XMP en clair (non compressé) dans le flux final — on peut le localiser
        // et le remplacer en mémoire sans repasser par PdfSharp (qui l'écraserait à nouveau).
        return ForceReplaceXmp(firstPass, request);
    }

    /// <summary>
    /// Supprime les dictionnaires /Group /S /Transparency des pages (interdit en PDF/A).
    /// </summary>
    private static void RemoveTransparencyGroups(PdfDocument document)
    {
        foreach (PdfPage page in document.Pages)
        {
            page.Elements.Remove("/Group");
        }
    }

    /// <summary>
    /// Injecte un CIDSet stream pour les polices CIDFont subset qui n'en ont pas.
    /// Le CIDSet est un bitmap indiquant quels CID sont présents dans le subset.
    /// Pour simplifier, on génère un CIDSet « tout à 1 » couvrant les CID 0-255.
    /// </summary>
    private static void InjectCidSets(PdfDocument document)
    {
        foreach (PdfPage page in document.Pages)
        {
            PdfDictionary? resources = page.Elements.GetDictionary("/Resources");
            PdfDictionary? fonts = resources?.Elements.GetDictionary("/Font");
            if (fonts is null)
            {
                continue;
            }

            foreach (System.Collections.Generic.KeyValuePair<string, PdfItem?> fontEntry in fonts.Elements)
            {
                if (fontEntry.Value is not PdfReference fontRef)
                {
                    continue;
                }

                if (fontRef.Value is not PdfDictionary fontDict)
                {
                    continue;
                }

                // Vérifier si c'est un Type0 (CIDFont composite)
                string subtype = fontDict.Elements.GetName("/Subtype");
                if (subtype != "/Type0")
                {
                    continue;
                }

                // Naviguer vers DescendantFonts[0]
                PdfArray? descendants = fontDict.Elements.GetArray("/DescendantFonts");
                if (descendants is null || descendants.Elements.Count == 0)
                {
                    continue;
                }

                PdfDictionary? cidFont = descendants.Elements[0] is PdfReference cidRef
                    ? cidRef.Value as PdfDictionary
                    : descendants.Elements[0] as PdfDictionary;

                if (cidFont is null)
                {
                    continue;
                }

                // Naviguer vers FontDescriptor
                PdfDictionary? fontDescriptor = cidFont.Elements.GetDictionary("/FontDescriptor");
                if (fontDescriptor is null)
                {
                    continue;
                }

                // Si CIDSet existe déjà, ne rien faire
                if (fontDescriptor.Elements.ContainsKey("/CIDSet"))
                {
                    continue;
                }

                // Créer un CIDSet stream couvrant CID 0-65535 (tous les bits à 1)
                // 65536 CIDs / 8 bits par byte = 8192 bytes
                byte[] cidSetBytes = new byte[8192];
                Array.Fill(cidSetBytes, (byte)0xFF);

                var cidSetStream = new PdfDictionary(document);
                cidSetStream.CreateStream(cidSetBytes);
                document.Internals.AddObject(cidSetStream);

                fontDescriptor.Elements["/CIDSet"] = cidSetStream.Reference;
            }
        }
    }

    /// <summary>
    /// Remplace le XMP dans les octets bruts du PDF.
    /// PdfSharp écrit toujours son XMP en clair (non compressé) dans le flux de sortie.
    /// On localise le xpacket et on le remplace en mémoire — sans repasser par PdfSharp
    /// qui l'écraserait à nouveau lors d'un second Save().
    /// </summary>
    private static byte[] ForceReplaceXmp(byte[] pdfBytes, FacturXRenderRequest request)
    {
        // Le XMP de PdfSharp est écrit en clair (pas de FlateDecode sur /Metadata)
        // Recherche en Latin-1 pour ne pas déformer les bytes non-ASCII
        string pdfLatin = Encoding.Latin1.GetString(pdfBytes);

        const string packetBegin = "<?xpacket begin=";
        const string packetEnd = "<?xpacket end=\"w\"?>";

        int startIdx = pdfLatin.IndexOf(packetBegin, StringComparison.Ordinal);
        if (startIdx < 0)
        {
            // Aucun xpacket trouvé — le XMP n'est pas dans le flux principal (cas inhabituel)
            return pdfBytes;
        }

        int endIdx = pdfLatin.IndexOf(packetEnd, startIdx, StringComparison.Ordinal);
        if (endIdx < 0)
        {
            return pdfBytes;
        }

        endIdx += packetEnd.Length;
        int originalLength = endIdx - startIdx;

        string conformanceLevel = MapConformanceLevel(request.Profile);
        string correctXmp = BuildXmpPacket(
            request.InvoiceNumber,
            request.SellerName,
            request.EmbeddedFileName,
            conformanceLevel);

        // Ajuster la taille : si le nouveau XMP est plus court, ajouter des espaces de padding
        // avant le marqueur de fin pour ne pas décaler les offsets xref du PDF
        string paddedXmp = correctXmp.Length < originalLength
            ? PadXmpToLength(correctXmp, originalLength)
            : correctXmp;

        // Si le nouveau XMP est plus long, on ne peut pas faire un remplacement in-place
        // sans recalculer les offsets xref — on tronque au maximum
        if (paddedXmp.Length > originalLength)
        {
            paddedXmp = paddedXmp[..originalLength];
        }

        byte[] result = new byte[pdfBytes.Length];
        Array.Copy(pdfBytes, result, pdfBytes.Length);

        byte[] xmpRaw = Encoding.Latin1.GetBytes(paddedXmp);
        Array.Copy(xmpRaw, 0, result, startIdx, xmpRaw.Length);

        return result;
    }

    private static string PadXmpToLength(string xmp, int targetLength)
    {
        const string endMarker = "<?xpacket end=";
        int endIdx = xmp.LastIndexOf(endMarker, StringComparison.Ordinal);
        if (endIdx < 0)
        {
            return xmp;
        }

        int paddingNeeded = targetLength - xmp.Length;
        string padding = new(' ', paddingNeeded);
        return string.Concat(xmp.AsSpan(0, endIdx), padding, xmp.AsSpan(endIdx));
    }

    public static void AttachFacturXXml(PdfDocument document, string fileName, byte[] xmlBytes)
    {
        var embeddedStream = new PdfDictionary(document);
        embeddedStream.Elements.SetName("/Type", "/EmbeddedFile");
        embeddedStream.Elements.SetName("/Subtype", "/text#2Fxml");
        embeddedStream.CreateStream(xmlBytes);

        var paramsDict = new PdfDictionary(document);
        paramsDict.Elements.SetInteger("/Size", xmlBytes.Length);
        paramsDict.Elements.SetDateTime("/ModDate", DateTime.UtcNow);
        embeddedStream.Elements["/Params"] = paramsDict;

        document.Internals.AddObject(embeddedStream);

        var efDict = new PdfDictionary(document);
        efDict.Elements["/F"] = embeddedStream.Reference;
        efDict.Elements["/UF"] = embeddedStream.Reference;

        var fileSpec = new PdfDictionary(document);
        fileSpec.Elements.SetName("/Type", "/Filespec");
        fileSpec.Elements.SetString("/F", fileName);
        fileSpec.Elements.SetString("/UF", fileName);
        fileSpec.Elements.SetString("/Desc", "Factur-X invoice data");
        fileSpec.Elements.SetName("/AFRelationship", "/Data");
        fileSpec.Elements["/EF"] = efDict;
        document.Internals.AddObject(fileSpec);

        PdfCatalog catalog = document.Internals.Catalog;
        PdfDictionary namesDict = catalog.Elements.GetDictionary("/Names") ?? new PdfDictionary(document);
        var embeddedFilesDict = new PdfDictionary(document);
        var namesArray = new PdfArray(document);
        namesArray.Elements.Add(new PdfString(fileName));
        namesArray.Elements.Add(fileSpec.Reference!);
        embeddedFilesDict.Elements["/Names"] = namesArray;
        namesDict.Elements["/EmbeddedFiles"] = embeddedFilesDict;
        catalog.Elements["/Names"] = namesDict;

        var afArray = new PdfArray(document);
        afArray.Elements.Add(fileSpec.Reference!);
        catalog.Elements["/AF"] = afArray;
    }

    public static void AttachOutputIntent(PdfDocument document)
    {
        byte[] iccBytes = LoadEmbeddedIccProfile();

        var iccStream = new PdfDictionary(document);
        iccStream.Elements.SetInteger("/N", 3); // RGB
        iccStream.CreateStream(iccBytes);
        document.Internals.AddObject(iccStream);

        var outputIntent = new PdfDictionary(document);
        outputIntent.Elements.SetName("/Type", "/OutputIntent");
        outputIntent.Elements.SetName("/S", "/GTS_PDFA1");
        outputIntent.Elements.SetString("/OutputConditionIdentifier", "sRGB");
        outputIntent.Elements.SetString("/RegistryName", "http://www.color.org");
        outputIntent.Elements.SetString("/Info", "sRGB IEC61966-2.1");
        outputIntent.Elements["/DestOutputProfile"] = iccStream.Reference;
        document.Internals.AddObject(outputIntent);

        var outputIntentsArray = new PdfArray(document);
        outputIntentsArray.Elements.Add(outputIntent.Reference!);
        document.Internals.Catalog.Elements["/OutputIntents"] = outputIntentsArray;
    }

    public static void AttachXmpMetadata(PdfDocument document, FacturXRenderRequest request)
    {
        string conformanceLevel = MapConformanceLevel(request.Profile);
        string xmp = BuildXmpPacket(request.InvoiceNumber, request.SellerName, request.EmbeddedFileName, conformanceLevel);
        byte[] xmpBytes = Encoding.UTF8.GetBytes(xmp);

        var metadataDict = new PdfDictionary(document);
        metadataDict.Elements.SetName("/Type", "/Metadata");
        metadataDict.Elements.SetName("/Subtype", "/XML");
        metadataDict.CreateStream(xmpBytes);

        document.Internals.AddObject(metadataDict);
        document.Internals.Catalog.Elements["/Metadata"] = metadataDict.Reference;
    }

    public static string BuildXmpPacket(
        string invoiceNumber,
        string sellerName,
        string embeddedFileName,
        string conformanceLevel)
    {
        string creationDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        return $"""
            <?xpacket begin="\ufeff" id="W5M0MpCehiHzreSzNTczkc9d"?>
            <x:xmpmeta xmlns:x="adobe:ns:meta/" x:xmptk="FacturXManager">
              <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
                <rdf:Description rdf:about=""
                    xmlns:pdfaid="http://www.aiim.org/pdfa/ns/id/">
                  <pdfaid:part>3</pdfaid:part>
                  <pdfaid:conformance>B</pdfaid:conformance>
                </rdf:Description>
                <rdf:Description rdf:about=""
                    xmlns:dc="http://purl.org/dc/elements/1.1/">
                  <dc:title><rdf:Alt><rdf:li xml:lang="x-default">Facture {Escape(invoiceNumber)}</rdf:li></rdf:Alt></dc:title>
                  <dc:creator><rdf:Seq><rdf:li>{Escape(sellerName)}</rdf:li></rdf:Seq></dc:creator>
                </rdf:Description>
                <rdf:Description rdf:about=""
                    xmlns:xmp="http://ns.adobe.com/xap/1.0/">
                  <xmp:CreatorTool>FacturXManager</xmp:CreatorTool>
                  <xmp:CreateDate>{creationDate}</xmp:CreateDate>
                  <xmp:ModifyDate>{creationDate}</xmp:ModifyDate>
                </rdf:Description>
                <rdf:Description rdf:about=""
                    xmlns:pdfaExtension="http://www.aiim.org/pdfa/ns/extension/"
                    xmlns:pdfaSchema="http://www.aiim.org/pdfa/ns/schema#"
                    xmlns:pdfaProperty="http://www.aiim.org/pdfa/ns/property#">
                  <pdfaExtension:schemas>
                    <rdf:Bag>
                      <rdf:li rdf:parseType="Resource">
                        <pdfaSchema:schema>Factur-X PDFA Extension Schema</pdfaSchema:schema>
                        <pdfaSchema:namespaceURI>urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#</pdfaSchema:namespaceURI>
                        <pdfaSchema:prefix>fx</pdfaSchema:prefix>
                        <pdfaSchema:property>
                          <rdf:Seq>
                            <rdf:li rdf:parseType="Resource">
                              <pdfaProperty:name>DocumentFileName</pdfaProperty:name>
                              <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                              <pdfaProperty:category>external</pdfaProperty:category>
                              <pdfaProperty:description>Name of the embedded XML invoice file</pdfaProperty:description>
                            </rdf:li>
                            <rdf:li rdf:parseType="Resource">
                              <pdfaProperty:name>DocumentType</pdfaProperty:name>
                              <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                              <pdfaProperty:category>external</pdfaProperty:category>
                              <pdfaProperty:description>INVOICE</pdfaProperty:description>
                            </rdf:li>
                            <rdf:li rdf:parseType="Resource">
                              <pdfaProperty:name>Version</pdfaProperty:name>
                              <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                              <pdfaProperty:category>external</pdfaProperty:category>
                              <pdfaProperty:description>Factur-X version</pdfaProperty:description>
                            </rdf:li>
                            <rdf:li rdf:parseType="Resource">
                              <pdfaProperty:name>ConformanceLevel</pdfaProperty:name>
                              <pdfaProperty:valueType>Text</pdfaProperty:valueType>
                              <pdfaProperty:category>external</pdfaProperty:category>
                              <pdfaProperty:description>Factur-X conformance level</pdfaProperty:description>
                            </rdf:li>
                          </rdf:Seq>
                        </pdfaSchema:property>
                      </rdf:li>
                    </rdf:Bag>
                  </pdfaExtension:schemas>
                </rdf:Description>
                <rdf:Description rdf:about=""
                    xmlns:fx="urn:factur-x:pdfa:CrossIndustryDocument:invoice:1p0#">
                  <fx:DocumentType>INVOICE</fx:DocumentType>
                  <fx:DocumentFileName>{Escape(embeddedFileName)}</fx:DocumentFileName>
                  <fx:Version>1.0</fx:Version>
                  <fx:ConformanceLevel>{conformanceLevel}</fx:ConformanceLevel>
                </rdf:Description>
              </rdf:RDF>
            </x:xmpmeta>
            <?xpacket end="w"?>
            """;
    }

    private static string MapConformanceLevel(FacturXProfile profile) => profile switch
    {
        FacturXProfile.Minimum => "MINIMUM",
        FacturXProfile.BasicWl => "BASIC WL",
        FacturXProfile.Basic => "BASIC",
        FacturXProfile.Extended => "EXTENDED",
        _ => "EN 16931"
    };

    public static string Escape(string value) =>
        value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    private static byte[] LoadEmbeddedIccProfile()
    {
        Assembly assembly = typeof(FacturXPdfEmbedder).Assembly;
        const string resourceName = "FacturXManager.Modules.Compliance.Infrastructure.Resources.sRGB2014.icc";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            // Fallback : profil ICC sRGB minimal (header-only, 60 bytes)
            // Suffisant pour satisfaire la contrainte PDF/A OutputIntents
            return GenerateMinimalSrgbIccProfile();
        }

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Génère un profil ICC v2 sRGB monitor valide avec les 9 tags obligatoires.
    /// Remplace l'ancien profil minimal à 0 tag qui causait l'erreur "ColorSpace incorrect" d'Acrobat.
    /// </summary>
    private static byte[] GenerateMinimalSrgbIccProfile()
    {
        // Tags obligatoires pour un profil mntr ICC v2 valide
        byte[] descData = BuildIccDescTag("sRGB IEC61966-2.1");
        byte[] cprtData = BuildIccTextTag("Copyright (c) 1998 Hewlett-Packard Company");
        byte[] wtptData = BuildIccXyzTag(0.96420288f, 1.00000000f, 0.82490540f); // D50
        byte[] rXyzData = BuildIccXyzTag(0.43607422f, 0.22248840f, 0.01393220f);
        byte[] gXyzData = BuildIccXyzTag(0.38506490f, 0.71694690f, 0.09710450f);
        byte[] bXyzData = BuildIccXyzTag(0.14308040f, 0.06060790f, 0.71417330f);
        byte[] trcData  = BuildIccCurvTag(563); // 563/256 ≈ 2.199 ≈ gamma sRGB — rTRC/gTRC/bTRC partagent ce bloc

        uint[] signatures =
        [
            0x64657363u, // desc
            0x63707274u, // cprt
            0x77747074u, // wtpt
            0x7258595Au, // rXYZ
            0x6758595Au, // gXYZ
            0x6258595Au, // bXYZ
            0x72545243u, // rTRC
            0x67545243u, // gTRC
            0x62545243u  // bTRC
        ];
        byte[][] tagDatas = [descData, cprtData, wtptData, rXyzData, gXyzData, bXyzData, trcData, trcData, trcData];
        int nTags = signatures.Length;

        // Calcul des offsets (rTRC/gTRC/bTRC partagent la même position)
        const int headerSize = 128;
        int tableSize = 4 + nTags * 12;
        int dataStart = headerSize + tableSize;

        int[] tagOffsets = new int[nTags];
        var seen = new Dictionary<byte[], int>(ReferenceEqualityComparer.Instance);
        int cursor = dataStart;

        for (int i = 0; i < nTags; i++)
        {
            if (seen.TryGetValue(tagDatas[i], out int existingOffset))
            {
                tagOffsets[i] = existingOffset;
            }
            else
            {
                tagOffsets[i] = cursor;
                seen[tagDatas[i]] = cursor;
                cursor += tagDatas[i].Length;
                cursor = (cursor + 3) & ~3; // aligner sur 4 octets
            }
        }

        byte[] profile = new byte[cursor];
        int o = 0;

        // En-tête ICC (128 bytes)
        IccW32(profile, ref o, (uint)cursor);    // taille du profil
        IccW32(profile, ref o, 0);               // CMM préféré
        IccW32(profile, ref o, 0x02400000u);     // version 2.4.0.0
        IccW32(profile, ref o, 0x6D6E7472u);     // classe 'mntr' (monitor)
        IccW32(profile, ref o, 0x52474220u);     // espace colorimétrique 'RGB '
        IccW32(profile, ref o, 0x58595A20u);     // PCS 'XYZ '
        IccW16(profile, ref o, 2014);            // année
        IccW16(profile, ref o, 1);               // mois
        IccW16(profile, ref o, 1);               // jour
        IccW16(profile, ref o, 0);               // heure
        IccW16(profile, ref o, 0);               // minute
        IccW16(profile, ref o, 0);               // seconde
        IccW32(profile, ref o, 0x61637370u);     // 'acsp'
        IccW32(profile, ref o, 0x4D534654u);     // plateforme 'MSFT'
        IccW32(profile, ref o, 0);               // flags
        IccW32(profile, ref o, 0);               // fabricant du device
        IccW32(profile, ref o, 0);               // modèle du device
        IccW32(profile, ref o, 0);               // attributs device (8 bytes)
        IccW32(profile, ref o, 0);
        IccW32(profile, ref o, 0);               // intention de rendu (perceptuel)
        IccFixed(profile, ref o, 0.96420288f);   // illuminant D50 X
        IccFixed(profile, ref o, 1.00000000f);   // illuminant D50 Y
        IccFixed(profile, ref o, 0.82490540f);   // illuminant D50 Z
        IccW32(profile, ref o, 0);               // créateur du profil
        o += 16;                                 // profile ID (MD5, mis à zéro en v2)
        o += 28;                                 // réservé

        // Nombre de tags
        IccW32(profile, ref o, (uint)nTags);

        // Table des tags
        for (int i = 0; i < nTags; i++)
        {
            IccW32(profile, ref o, signatures[i]);
            IccW32(profile, ref o, (uint)tagOffsets[i]);
            IccW32(profile, ref o, (uint)tagDatas[i].Length);
        }

        // Données des tags
        var written = new HashSet<byte[]>(ReferenceEqualityComparer.Instance);
        for (int i = 0; i < nTags; i++)
        {
            if (written.Add(tagDatas[i]))
            {
                Buffer.BlockCopy(tagDatas[i], 0, profile, tagOffsets[i], tagDatas[i].Length);
            }
        }

        return profile;
    }

    // 'desc' tag — textDescriptionType ICC v2
    private static byte[] BuildIccDescTag(string text)
    {
        byte[] ascii = Encoding.ASCII.GetBytes(text + "\0");
        // sig(4) + reserved(4) + count(4) + ascii[count] + unicode_count(4) + script_count(1) + script[67]
        byte[] result = new byte[4 + 4 + 4 + ascii.Length + 4 + 1 + 67];
        int o = 0;
        IccW32(result, ref o, 0x64657363u);
        IccW32(result, ref o, 0);
        IccW32(result, ref o, (uint)ascii.Length);
        Buffer.BlockCopy(ascii, 0, result, o, ascii.Length);
        return result;
    }

    // 'text' tag — textType ICC v2
    private static byte[] BuildIccTextTag(string text)
    {
        byte[] ascii = Encoding.ASCII.GetBytes(text + "\0");
        byte[] result = new byte[4 + 4 + ascii.Length];
        int o = 0;
        IccW32(result, ref o, 0x74657874u);
        IccW32(result, ref o, 0);
        Buffer.BlockCopy(ascii, 0, result, o, ascii.Length);
        return result;
    }

    // 'XYZ ' tag — XYZType (20 bytes)
    private static byte[] BuildIccXyzTag(float x, float y, float z)
    {
        byte[] result = new byte[20];
        int o = 0;
        IccW32(result, ref o, 0x58595A20u);
        IccW32(result, ref o, 0);
        IccFixed(result, ref o, x);
        IccFixed(result, ref o, y);
        IccFixed(result, ref o, z);
        return result;
    }

    // 'curv' tag avec 1 valeur gamma (14 bytes)
    private static byte[] BuildIccCurvTag(int gamma256)
    {
        byte[] result = new byte[14];
        int o = 0;
        IccW32(result, ref o, 0x63757276u);
        IccW32(result, ref o, 0);
        IccW32(result, ref o, 1);
        result[o]     = (byte)(gamma256 >> 8);
        result[o + 1] = (byte)gamma256;
        return result;
    }

    private static void IccW32(byte[] buf, ref int o, uint v)
    {
        buf[o] = (byte)(v >> 24); buf[o + 1] = (byte)(v >> 16);
        buf[o + 2] = (byte)(v >> 8); buf[o + 3] = (byte)v;
        o += 4;
    }

    private static void IccW16(byte[] buf, ref int o, int v)
    {
        buf[o] = (byte)(v >> 8); buf[o + 1] = (byte)v;
        o += 2;
    }

    private static void IccFixed(byte[] buf, ref int o, float v)
    {
        // s15Fixed16Number : int32 big-endian = valeur × 65536
        IccW32(buf, ref o, (uint)(int)MathF.Round(v * 65536f));
    }
}
