import React, { useState, useEffect } from "react";
import Editor from "@monaco-editor/react";
import axios from "axios";

const EditorWithControls: React.FC = () => {
  const [language, setLanguage] = useState<string>("python");
  const [theme, setTheme] = useState<string>("vs-dark");
  const [code, setCode] = useState<string>("print(\"Hello World\")");
  const [history, setHistory] = useState<Script[]>([]);
  const [output, setOutput] = useState<string>("");

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
      setOutput(res.data.output || "");
    } catch (err) {
      setOutput(`Error running code: ${err}`);
    }
  };

  const saveScript = async () => {
    if (!code.trim()) return;
    try {
      const lang = language === "csharp" ? "C#" : "Python";
      const res = await axios.post("/CodeExecution/save", { language: lang, code });
      setHistory(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const loadHistory = async () => {
    try {
      const lang = language === "csharp" ? "C#" : "Python";
      const res = await axios.get(`/CodeExecution/history/${encodeURIComponent(lang)}`);
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

  // Load history on mount
  useEffect(() => {
    loadHistory();
  }, []);

  // Clear code, load history on mount or reload history when language changes
  useEffect(() => {
    setCode("");
    loadHistory();
  }, [language]);

  return (
    <div style={{ display: "flex", flexDirection: "column", height: "100vh", padding: "10px" }}>

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
        <button onClick={saveScript}>Save</button>
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

      {/* History */}
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
    </div>
  );
};

export default EditorWithControls;