import React, { useState, useEffect, useRef } from "react";
import Editor from "@monaco-editor/react";
import axios from "axios";
import { useAuth } from "../context/AuthProvider";

const examples: Record<string, string> = {
  python: `nums = []

for i in range(5):
    for j in range(5):
        nums.append(i+j)

print(nums)`,
  csharp: `using System.Collections.Generic;

List<int> list = new List<int>();

for (int i = 0; i < 5; i++)
{
    for (int j = 0; j < 5; j++)
    {
        list.Add(i + j);
    }
}

string result = string.Join(", ", list);
result`
};

const EditorWithControls: React.FC = () => {
  const { user, logout, setShowAuth } = useAuth();
  const [language, setLanguage] = useState<string>("python");
  const [theme, setTheme] = useState<string>("vs-dark");
  const [code, setCode] = useState<string>("print(\"Hello World\")");
  const [history, setHistory] = useState<Script[]>([]);
  const [output, setOutput] = useState<string>("");

  // useRef is used here to store a persistent cache of history per language.
  // This way, the cache survives component re-renders without causing extra renders.
  const historyCache = useRef<Record<string, Script[]>>({});

  interface Script {
    id: number;
    code: string;
    createdAt: string;
    language: string;
  }

  const runCode = async () => {
    try {
      const res = await axios.post("/CodeExecution", {
        language: language === "python" ? "Python" : "C#",
        code: code
      });
      const time = res.data.executionTimeMs;
      const out = res.data.output || "";
      setOutput(time != null ? `${out}\n\n⏱ Execution time: ${time}ms` : out);
    } catch (err) {
      setOutput(`Error running code: ${err}`);
    }
  };

  const saveScript = async () => {
    if (!code.trim()) return;
    try {
      const lang = language === "csharp" ? "C#" : "Python";
      const res = await axios.post("/CodeExecution/save", { language: lang, code });
      historyCache.current[lang] = res.data;
      setHistory(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const loadHistory = async () => {
    const lang = language === "csharp" ? "C#" : "Python";

    if (historyCache.current[lang]) {
      setHistory(historyCache.current[lang]);
      return;
    }

    try {
      const res = await axios.get(`/CodeExecution/history/${encodeURIComponent(lang)}`);
      historyCache.current[lang] = res.data;
      setHistory(res.data);
    } catch (err) {
      console.error("Failed to load history", err);
    }
  };

  // Add F5 shortcut to run code
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "F5") {
        e.preventDefault(); // prevent browser refresh
        runCode();
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [code, language]);

  // Load history on mount (only if logged in)
  useEffect(() => {
    if (user) loadHistory();
  }, []);

  // Clear output when user logs in or out
  useEffect(() => {
    setOutput("");
  }, [user]);

  // Clear code, output, and reload history when language changes
  useEffect(() => {
    setCode("");
    setOutput("");
    if (user) loadHistory();
  }, [language]);

  return (
    <div style={{ display: "flex", flexDirection: "column", height: "100vh", padding: "10px" }}>

      {/* Top bar */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "10px", padding: "6px 12px", background: "#2d2d2d", borderRadius: "6px" }}>
        <span style={{ color: "#ccc", fontSize: "14px" }}>MiniCloud IDE</span>
        {user ? (
          <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
            <span style={{ color: "#ccc", fontSize: "14px" }}>Welcome, <strong style={{ color: "#fff" }}>{user.username}</strong></span>
            <button onClick={logout} style={{ padding: "6px 16px", background: "#dc3545", color: "#fff", border: "none", borderRadius: "4px", cursor: "pointer" }}>Logout</button>
          </div>
        ) : (
          <button onClick={() => setShowAuth(true)} style={{ padding: "6px 16px", background: "#0078d4", color: "#fff", border: "none", borderRadius: "4px", cursor: "pointer" }}>Login / Register</button>
        )}
      </div>

      {/* Controls */}
      <div style={{ marginBottom: "10px", display: "flex", gap: "10px", alignItems: "center" }}>
        <label>
          Language:
          <select value={language} onChange={(e) => setLanguage(e.target.value)}>
            <option value="python">Python</option>
            <option value="csharp">C#</option>
          </select>
        </label>

        <label>
          Theme:
          <select value={theme} onChange={(e) => setTheme(e.target.value)}>
            <option value="vs-dark">Dark</option>
            <option value="light">Light</option>
          </select>
        </label>

        {/* Run button */}
        <button onClick={runCode}>Run (F5)</button>
        {user && <button onClick={saveScript}>Save</button>}
        <button onClick={() => setCode(examples[language])}>Load Example</button>
      </div>

      {/* Editor */}
      <div style={{ flexGrow: 1, minHeight: "300px" }}>
        <Editor
          height="100%"
          language={language}
          theme={theme}
          value={code}
          onChange={(value) => setCode(value || "")}
          options={{
            fontSize: 16,
            minimap: { enabled: false },
            automaticLayout: true,
          }}
        />
      </div>

      {/* Output */}
      <pre style={{ marginTop: "10px", background: "#f0f0f0", padding: "10px", minHeight: "100px" }}>
        {output}
      </pre>

      {/* History (only for logged in users) */}
      {user && (
        <div style={{ marginTop: "10px" }}>
          <h3>History</h3>
          {history.length === 0 ? (
            <p>No scripts saved yet.</p>
          ) : (
            history.map((script) => (
              <div key={script.id} style={{ marginBottom: "5px" }}>
                <button
                  onClick={() => setCode(script.code)}
                  style={{ padding: "5px 10px", cursor: "pointer" }}
                >
                  {new Date(script.createdAt).toLocaleString()} — Load
                </button>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
};

export default EditorWithControls;