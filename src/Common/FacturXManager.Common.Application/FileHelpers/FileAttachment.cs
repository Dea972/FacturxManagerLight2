using System;
using System.Collections.Generic;
using System.Text;

namespace FacturXManager.Common.Application.FileHelpers;

public sealed record FileAttachment(
    string FileName,
    string ContentType,
    byte[] Content);