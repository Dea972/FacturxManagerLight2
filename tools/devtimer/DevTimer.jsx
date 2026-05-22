import { useState, useEffect, useRef, useCallback } from "react";

const CATEGORIES = [
  { id: "xml", label: "XML / Factur-X", color: "#00d4ff" },
  { id: "pdf", label: "PDF/A-3", color: "#ff6b35" },
  { id: "chorus", label: "Chorus Pro / PISTE", color: "#7c3aed" },
  { id: "tests", label: "Tests", color: "#10b981" },
  { id: "arch", label: "Architecture", color: "#f59e0b" },
  { id: "autre", label: "Autre", color: "#6b7280" },
];

function formatDuration(ms) {
  const totalSec = Math.floor(ms / 1000);
  const h = Math.floor(totalSec / 3600);
  const m = Math.floor((totalSec % 3600) / 60);
  const s = totalSec % 60;
  if (h > 0) return `${h}h ${String(m).padStart(2, "0")}m ${String(s).padStart(2, "0")}s`;
  return `${String(m).padStart(2, "0")}m ${String(s).padStart(2, "0")}s`;
}

function formatTime(date) {
  return new Date(date).toLocaleTimeString("fr-FR", { hour: "2-digit", minute: "2-digit" });
}

function formatDate(date) {
  return new Date(date).toLocaleDateString("fr-FR", { day: "2-digit", month: "2-digit", year: "2-digit" });
}

const STORAGE_KEY = "facturx_devtimer_v1";

function loadState() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) return JSON.parse(raw);
  } catch {}
  return { sessions: [] };
}

function saveState(state) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  } catch {}
}

export default function DevTimer() {
  const [sessions, setSessions] = useState(() => loadState().sessions);
  const [running, setRunning] = useState(false);
  const [elapsed, setElapsed] = useState(0);
  const [startTime, setStartTime] = useState(null);
  const [label, setLabel] = useState("");
  const [category, setCategory] = useState("xml");
  const [activeTab, setActiveTab] = useState("timer");
  const [filterCat, setFilterCat] = useState("all");
  const intervalRef = useRef(null);

  useEffect(() => {
    saveState({ sessions });
  }, [sessions]);

  useEffect(() => {
    if (running) {
      intervalRef.current = setInterval(() => {
        setElapsed(Date.now() - startTime);
      }, 500);
    } else {
      clearInterval(intervalRef.current);
    }
    return () => clearInterval(intervalRef.current);
  }, [running, startTime]);

  const handleStart = () => {
    const now = Date.now();
    setStartTime(now);
    setElapsed(0);
    setRunning(true);
  };

  const handleStop = () => {
    setRunning(false);
    const duration = Date.now() - startTime;
    if (duration < 1000) return;
    const session = {
      id: Date.now(),
      label: label.trim() || "Session sans titre",
      category,
      startTime,
      duration,
    };
    setSessions((prev) => [session, ...prev]);
    setElapsed(0);
    setLabel("");
  };

  const handleDelete = (id) => {
    setSessions((prev) => prev.filter((s) => s.id !== id));
  };

  const exportCSV = () => {
    const header = "Date,Heure début,Tâche,Catégorie,Durée (min)\n";
    const rows = sessions.map((s) => {
      const cat = CATEGORIES.find((c) => c.id === s.category)?.label || s.category;
      const mins = (s.duration / 60000).toFixed(1);
      return `${formatDate(s.startTime)},${formatTime(s.startTime)},"${s.label}","${cat}",${mins}`;
    });
    const blob = new Blob([header + rows.join("\n")], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `devtime_facturx_${new Date().toISOString().slice(0, 10)}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const filtered = filterCat === "all" ? sessions : sessions.filter((s) => s.category === filterCat);

  const totalByCategory = CATEGORIES.map((cat) => ({
    ...cat,
    total: sessions.filter((s) => s.category === cat.id).reduce((a, s) => a + s.duration, 0),
  })).filter((c) => c.total > 0);

  const grandTotal = sessions.reduce((a, s) => a + s.duration, 0);

  const cat = CATEGORIES.find((c) => c.id === category);

  return (
    <div style={{
      minHeight: "100vh",
      background: "#0a0a0f",
      color: "#e8e8f0",
      fontFamily: "'JetBrains Mono', 'Fira Code', 'Courier New', monospace",
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      padding: "32px 16px",
    }}>
      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@300;400;500;700&family=Space+Grotesk:wght@300;600;700&display=swap');
        * { box-sizing: border-box; }
        ::-webkit-scrollbar { width: 4px; }
        ::-webkit-scrollbar-track { background: #0a0a0f; }
        ::-webkit-scrollbar-thumb { background: #2a2a3f; border-radius: 2px; }
        .tab-btn { background: none; border: none; cursor: pointer; padding: 8px 20px; font-family: inherit; font-size: 12px; letter-spacing: 0.1em; text-transform: uppercase; transition: all 0.2s; }
        .tab-btn.active { color: #00d4ff; border-bottom: 2px solid #00d4ff; }
        .tab-btn:not(.active) { color: #4a4a6a; border-bottom: 2px solid transparent; }
        .tab-btn:not(.active):hover { color: #8888aa; }
        .cat-btn { border: 1px solid; border-radius: 4px; cursor: pointer; padding: 5px 12px; font-family: inherit; font-size: 11px; letter-spacing: 0.05em; transition: all 0.15s; }
        .session-row:hover { background: #12121c !important; }
        .del-btn { background: none; border: none; cursor: pointer; color: #3a3a5a; font-size: 14px; padding: 4px 8px; transition: color 0.15s; line-height: 1; }
        .del-btn:hover { color: #ff4466; }
        input[type=text]:focus { outline: none; border-color: #00d4ff !important; }
        @keyframes pulse-ring { 0%,100%{opacity:0.6;transform:scale(1)} 50%{opacity:1;transform:scale(1.03)} }
        @keyframes blink { 0%,100%{opacity:1} 50%{opacity:0.3} }
        .running-dot { animation: blink 1s infinite; }
      `}</style>

      {/* Header */}
      <div style={{ width: "100%", maxWidth: 640, marginBottom: 32 }}>
        <div style={{ display: "flex", alignItems: "baseline", gap: 12, marginBottom: 4 }}>
          <span style={{ fontSize: 11, letterSpacing: "0.2em", color: "#00d4ff", textTransform: "uppercase" }}>FacturX Manager</span>
          <span style={{ color: "#2a2a4a" }}>|</span>
          <span style={{ fontSize: 11, color: "#4a4a6a", letterSpacing: "0.1em" }}>Dev Timer</span>
        </div>
        <div style={{ height: 1, background: "linear-gradient(90deg, #00d4ff22, #7c3aed22, transparent)" }} />
      </div>

      {/* Main card */}
      <div style={{ width: "100%", maxWidth: 640, background: "#0e0e18", border: "1px solid #1e1e2e", borderRadius: 12 }}>

        {/* Tabs */}
        <div style={{ display: "flex", borderBottom: "1px solid #1e1e2e" }}>
          {[["timer", "⏱ Chrono"], ["history", `📋 Historique (${sessions.length})`], ["stats", "📊 Stats"]].map(([id, lbl]) => (
            <button key={id} className={`tab-btn ${activeTab === id ? "active" : ""}`} onClick={() => setActiveTab(id)}>{lbl}</button>
          ))}
        </div>

        {/* TIMER TAB */}
        {activeTab === "timer" && (
          <div style={{ padding: 32 }}>
            {/* Big clock */}
            <div style={{
              textAlign: "center",
              marginBottom: 32,
              padding: "32px 0",
              position: "relative",
            }}>
              {running && (
                <div style={{
                  position: "absolute", inset: 0, borderRadius: 8,
                  background: `radial-gradient(ellipse at center, ${cat.color}08 0%, transparent 70%)`,
                  animation: "pulse-ring 2s infinite",
                }} />
              )}
              <div style={{
                fontSize: 72,
                fontWeight: 700,
                letterSpacing: "-0.02em",
                color: running ? cat.color : "#3a3a5a",
                transition: "color 0.4s",
                lineHeight: 1,
                fontVariantNumeric: "tabular-nums",
              }}>
                {formatDuration(elapsed)}
              </div>
              {running && (
                <div style={{ marginTop: 8, display: "flex", alignItems: "center", justifyContent: "center", gap: 6, fontSize: 11, color: cat.color, opacity: 0.7 }}>
                  <span className="running-dot">●</span>
                  <span style={{ letterSpacing: "0.15em", textTransform: "uppercase" }}>En cours</span>
                </div>
              )}
            </div>

            {/* Inputs */}
            <div style={{ display: "flex", flexDirection: "column", gap: 12, marginBottom: 24 }}>
              <input
                type="text"
                placeholder="Nom de la tâche (ex: Implem OAuth2 PISTE)…"
                value={label}
                onChange={(e) => setLabel(e.target.value)}
                disabled={running}
                style={{
                  background: "#080810",
                  border: "1px solid #1e1e2e",
                  borderRadius: 6,
                  padding: "10px 14px",
                  color: "#e8e8f0",
                  fontFamily: "inherit",
                  fontSize: 13,
                  opacity: running ? 0.5 : 1,
                  transition: "border-color 0.2s",
                }}
              />
              {/* Category selector */}
              <div style={{ display: "flex", flexWrap: "wrap", gap: 8 }}>
                {CATEGORIES.map((c) => (
                  <button
                    key={c.id}
                    className="cat-btn"
                    disabled={running}
                    onClick={() => setCategory(c.id)}
                    style={{
                      borderColor: category === c.id ? c.color : "#1e1e2e",
                      color: category === c.id ? c.color : "#4a4a6a",
                      background: category === c.id ? `${c.color}12` : "transparent",
                      opacity: running ? 0.5 : 1,
                    }}
                  >
                    {c.label}
                  </button>
                ))}
              </div>
            </div>

            {/* Action button */}
            <button
              onClick={running ? handleStop : handleStart}
              style={{
                width: "100%",
                padding: "14px",
                borderRadius: 8,
                border: "none",
                cursor: "pointer",
                fontSize: 14,
                fontFamily: "inherit",
                fontWeight: 700,
                letterSpacing: "0.1em",
                textTransform: "uppercase",
                transition: "all 0.2s",
                background: running
                  ? "linear-gradient(135deg, #ff446622, #ff444411)"
                  : `linear-gradient(135deg, ${cat.color}22, ${cat.color}11)`,
                color: running ? "#ff4466" : cat.color,
                border: `1px solid ${running ? "#ff446644" : cat.color + "44"}`,
              }}
            >
              {running ? "⏹ Arrêter & enregistrer" : "▶ Démarrer"}
            </button>
          </div>
        )}

        {/* HISTORY TAB */}
        {activeTab === "history" && (
          <div style={{ padding: "16px 0" }}>
            {/* Filter bar */}
            <div style={{ display: "flex", gap: 8, padding: "0 20px 16px", flexWrap: "wrap", borderBottom: "1px solid #1a1a2e" }}>
              <button
                className="cat-btn"
                onClick={() => setFilterCat("all")}
                style={{ borderColor: filterCat === "all" ? "#e8e8f0" : "#1e1e2e", color: filterCat === "all" ? "#e8e8f0" : "#4a4a6a", background: "transparent" }}
              >Tout</button>
              {CATEGORIES.map((c) => (
                <button key={c.id} className="cat-btn"
                  onClick={() => setFilterCat(c.id)}
                  style={{ borderColor: filterCat === c.id ? c.color : "#1e1e2e", color: filterCat === c.id ? c.color : "#4a4a6a", background: filterCat === c.id ? `${c.color}12` : "transparent" }}>
                  {c.label}
                </button>
              ))}
            </div>

            {/* Sessions list */}
            <div style={{ maxHeight: 380, overflowY: "auto" }}>
              {filtered.length === 0 ? (
                <div style={{ padding: 40, textAlign: "center", color: "#2a2a4a", fontSize: 13 }}>
                  Aucune session enregistrée.
                </div>
              ) : (
                filtered.map((s) => {
                  const c = CATEGORIES.find((c) => c.id === s.category);
                  return (
                    <div key={s.id} className="session-row" style={{
                      display: "flex", alignItems: "center", gap: 12,
                      padding: "10px 20px", borderBottom: "1px solid #12121c",
                      transition: "background 0.15s",
                    }}>
                      <div style={{ width: 3, height: 36, borderRadius: 2, background: c?.color || "#4a4a6a", flexShrink: 0 }} />
                      <div style={{ flex: 1, minWidth: 0 }}>
                        <div style={{ fontSize: 13, color: "#c8c8e0", whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>{s.label}</div>
                        <div style={{ fontSize: 10, color: "#3a3a5a", marginTop: 2 }}>
                          {formatDate(s.startTime)} · {formatTime(s.startTime)}
                        </div>
                      </div>
                      <div style={{ fontSize: 13, color: c?.color || "#6b7280", fontWeight: 500, flexShrink: 0 }}>
                        {formatDuration(s.duration)}
                      </div>
                      <button className="del-btn" onClick={() => handleDelete(s.id)}>✕</button>
                    </div>
                  );
                })
              )}
            </div>

            {/* Export button */}
            {sessions.length > 0 && (
              <div style={{ padding: "16px 20px 4px", display: "flex", justifyContent: "flex-end" }}>
                <button onClick={exportCSV} style={{
                  background: "none", border: "1px solid #1e1e2e", borderRadius: 6,
                  color: "#4a4a6a", cursor: "pointer", fontFamily: "inherit",
                  fontSize: 11, padding: "6px 14px", letterSpacing: "0.08em",
                  transition: "all 0.15s",
                }}
                  onMouseEnter={(e) => { e.target.style.color = "#10b981"; e.target.style.borderColor = "#10b981"; }}
                  onMouseLeave={(e) => { e.target.style.color = "#4a4a6a"; e.target.style.borderColor = "#1e1e2e"; }}
                >
                  ↓ Exporter CSV
                </button>
              </div>
            )}
          </div>
        )}

        {/* STATS TAB */}
        {activeTab === "stats" && (
          <div style={{ padding: 24 }}>
            {sessions.length === 0 ? (
              <div style={{ padding: 40, textAlign: "center", color: "#2a2a4a", fontSize: 13 }}>
                Aucune donnée disponible.
              </div>
            ) : (
              <>
                <div style={{ marginBottom: 24, padding: 16, background: "#080810", borderRadius: 8, textAlign: "center" }}>
                  <div style={{ fontSize: 11, color: "#4a4a6a", letterSpacing: "0.15em", textTransform: "uppercase", marginBottom: 6 }}>Temps total</div>
                  <div style={{ fontSize: 40, fontWeight: 700, color: "#00d4ff" }}>{formatDuration(grandTotal)}</div>
                  <div style={{ fontSize: 11, color: "#3a3a5a", marginTop: 4 }}>{sessions.length} session{sessions.length > 1 ? "s" : ""}</div>
                </div>

                <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
                  {totalByCategory.sort((a, b) => b.total - a.total).map((c) => {
                    const pct = grandTotal > 0 ? (c.total / grandTotal) * 100 : 0;
                    const count = sessions.filter((s) => s.category === c.id).length;
                    return (
                      <div key={c.id}>
                        <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 5, fontSize: 12 }}>
                          <span style={{ color: c.color }}>{c.label}</span>
                          <span style={{ color: "#6a6a8a" }}>{formatDuration(c.total)} · {pct.toFixed(0)}% · {count} session{count > 1 ? "s" : ""}</span>
                        </div>
                        <div style={{ height: 4, background: "#1a1a2a", borderRadius: 2, overflow: "hidden" }}>
                          <div style={{
                            height: "100%", borderRadius: 2,
                            width: `${pct}%`,
                            background: c.color,
                            transition: "width 0.6s ease",
                          }} />
                        </div>
                      </div>
                    );
                  })}
                </div>
              </>
            )}
          </div>
        )}
      </div>

      {/* Footer */}
      <div style={{ marginTop: 20, fontSize: 10, color: "#2a2a3a", letterSpacing: "0.1em" }}>
        Données sauvegardées localement dans le navigateur
      </div>
    </div>
  );
}
