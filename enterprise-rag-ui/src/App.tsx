import React, { useState, useRef, useEffect } from 'react';
import { Upload, Send, FileText, Loader2, Bot, User, BrainCircuit, Sparkles, MessageSquare } from 'lucide-react';

// Adjust API_BASE_URL to match the backend port
const API_BASE_URL = 'http://localhost:5209/api/v1';

interface Message {
  id: string;
  sender: 'user' | 'ai';
  text: string;
  sources?: Array<{
    sourceFile: string;
    sequenceIndex: number;
    content: string;
  }>;
}

export default function App() {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [activeDocument, setActiveDocument] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadStatus, setUploadStatus] = useState<{ type: 'success' | 'error', message: string } | null>(null);

  const [messages, setMessages] = useState<Message[]>([
    {
      id: 'welcome',
      sender: 'ai',
      text: 'Hello! I am your Enterprise AI Assistant. Upload a PDF and ask me anything about it.',
    }
  ]);
  const [inputValue, setInputValue] = useState('');
  const [isSearching, setIsSearching] = useState(false);
  
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Auto-scroll to bottom of chat
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      setSelectedFile(e.target.files[0]);
      setUploadStatus(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    setIsUploading(true);
    setUploadStatus(null);
    
    const formData = new FormData();
    formData.append('file', selectedFile);

    try {
      const response = await fetch(`${API_BASE_URL}/ingestion/upload`, {
        method: 'POST',
        body: formData,
      });

      if (!response.ok) {
        throw new Error(`Upload failed with status: ${response.status}`);
      }

      setUploadStatus({ type: 'success', message: 'Document embedded successfully!' });
      setActiveDocument(selectedFile.name);
      setSelectedFile(null);
    } catch (error: any) {
      console.error(error);
      setUploadStatus({ type: 'error', message: error.message || 'An unexpected error occurred during upload.' });
    } finally {
      setIsUploading(false);
    }
  };

  const handleSendMessage = async () => {
    if (!inputValue.trim()) return;

    const userMessage: Message = {
      id: Date.now().toString(),
      sender: 'user',
      text: inputValue.trim()
    };

    setMessages(prev => [...prev, userMessage]);
    setInputValue('');
    setIsSearching(true);

    try {
      const response = await fetch(`${API_BASE_URL}/query/search`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ 
          userQuestion: userMessage.text,
          documentName: activeDocument
        })
      });

      if (!response.ok) {
        throw new Error(`Search failed with status: ${response.status}`);
      }

      const data = await response.json();
      
      const aiMessage: Message = {
        id: (Date.now() + 1).toString(),
        sender: 'ai',
        text: data.answer || "I'm sorry, I couldn't generate an answer.",
        sources: data.sources
      };

      setMessages(prev => [...prev, aiMessage]);
    } catch (error: any) {
      console.error(error);
      setMessages(prev => [...prev, {
        id: (Date.now() + 1).toString(),
        sender: 'ai',
        text: `Error: ${error.message || 'An unexpected error occurred during search.'}`
      }]);
    } finally {
      setIsSearching(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      handleSendMessage();
    }
  };

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex items-center justify-center p-4 sm:p-6 lg:p-8 relative overflow-hidden">
      
      {/* Animated Background Blobs */}
      <div className="absolute top-0 -left-4 w-72 h-72 bg-purple-500 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob"></div>
      <div className="absolute top-0 -right-4 w-72 h-72 bg-indigo-500 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-2000"></div>
      <div className="absolute -bottom-8 left-20 w-72 h-72 bg-blue-500 rounded-full mix-blend-multiply filter blur-3xl opacity-20 animate-blob animation-delay-4000"></div>

      {/* Main Glass Container */}
      <div className="w-full max-w-6xl h-[85vh] glass-panel rounded-3xl flex flex-col md:flex-row overflow-hidden relative z-10 border border-white/10 shadow-2xl">
        
        {/* Left Sidebar: Document Management */}
        <div className="w-full md:w-80 bg-slate-900/50 border-b md:border-b-0 md:border-r border-white/10 flex flex-col p-6">
          <div className="flex items-center gap-3 mb-8">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center shadow-lg shadow-indigo-500/30">
              <BrainCircuit className="w-6 h-6 text-white" />
            </div>
            <div>
              <h1 className="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-indigo-200 to-white">Enterprise RAG</h1>
              <p className="text-xs text-indigo-300/70 font-medium">AI Document Intelligence</p>
            </div>
          </div>

          <div className="flex-1 flex flex-col">
            <h2 className="text-sm font-semibold text-slate-300 uppercase tracking-wider mb-4 flex items-center gap-2">
              <FileText className="w-4 h-4 text-indigo-400" />
              Knowledge Base
            </h2>
            
            <div className="glass-panel rounded-2xl p-5 border border-white/5 flex flex-col items-center justify-center text-center gap-3 hover:border-indigo-500/30 transition-colors duration-300">
              <div className="w-12 h-12 rounded-full bg-indigo-500/10 flex items-center justify-center mb-1">
                <Upload className="w-5 h-5 text-indigo-400" />
              </div>
              <h3 className="font-medium text-slate-200">Upload Document</h3>
              <p className="text-xs text-slate-400 mb-2">PDF files up to 50MB</p>
              
              <label className="w-full glass-button cursor-pointer bg-white/5 hover:bg-white/10 text-indigo-300 text-sm font-medium py-2.5 px-4 rounded-xl border border-white/10 flex items-center justify-center gap-2">
                <input 
                  type="file" 
                  accept=".pdf" 
                  className="hidden" 
                  onChange={handleFileChange}
                />
                Choose File
              </label>

              {selectedFile && (
                <div className="w-full mt-3 p-3 rounded-xl bg-indigo-500/10 border border-indigo-500/20 text-left flex flex-col gap-2">
                  <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4 text-indigo-400 shrink-0" />
                    <p className="text-xs font-medium text-indigo-200 truncate">{selectedFile.name}</p>
                  </div>
                  <button 
                    onClick={handleUpload} 
                    disabled={isUploading}
                    className="w-full glass-button bg-gradient-to-r from-indigo-500 to-purple-600 text-white text-xs font-medium py-2 rounded-lg flex items-center justify-center gap-2 shadow-lg shadow-indigo-500/25"
                  >
                    {isUploading ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Sparkles className="w-3.5 h-3.5" />}
                    {isUploading ? 'Embedding...' : 'Process & Embed'}
                  </button>
                </div>
              )}

              {uploadStatus && (
                <div className={`w-full mt-3 p-3 rounded-xl text-xs font-medium border ${uploadStatus.type === 'success' ? 'bg-emerald-500/10 text-emerald-300 border-emerald-500/20' : 'bg-rose-500/10 text-rose-300 border-rose-500/20'}`}>
                  {uploadStatus.message}
                </div>
              )}
            </div>
          </div>
          
          <div className="mt-6 pt-6 border-t border-white/10">
            <div className="flex items-center gap-3">
              <div className="w-8 h-8 rounded-full bg-slate-800 border border-white/10 flex items-center justify-center">
                <User className="w-4 h-4 text-slate-400" />
              </div>
              <div>
                <p className="text-sm font-medium text-slate-200">Admin User</p>
                <p className="text-xs text-slate-500">System Administrator</p>
              </div>
            </div>
          </div>
        </div>

        {/* Right Main Area: Chat Interface */}
        <div className="flex-1 flex flex-col bg-slate-950/20 relative">
          
          {/* Header */}
          <div className="h-16 border-b border-white/10 flex items-center px-6 bg-slate-900/30 backdrop-blur-md sticky top-0 z-20">
            <div className="flex items-center gap-2">
              <MessageSquare className="w-5 h-5 text-indigo-400" />
              <h2 className="font-medium text-slate-200">AI Assistant Chat</h2>
            </div>
          </div>

          {/* Messages Area */}
          <div className="flex-1 overflow-y-auto p-6 flex flex-col gap-6 scroll-smooth">
            {messages.map((msg) => (
              <div key={msg.id} className={`flex gap-4 ${msg.sender === 'user' ? 'flex-row-reverse' : ''}`}>
                
                {/* Avatar */}
                <div className={`shrink-0 w-10 h-10 rounded-full flex items-center justify-center shadow-lg ${
                  msg.sender === 'user' 
                    ? 'bg-gradient-to-br from-indigo-500 to-purple-600 shadow-indigo-500/20' 
                    : 'bg-slate-800 border border-white/10 shadow-black/20'
                }`}>
                  {msg.sender === 'user' ? <User className="w-5 h-5 text-white" /> : <Bot className="w-5 h-5 text-indigo-300" />}
                </div>
                
                {/* Message Content */}
                <div className={`max-w-[80%] flex flex-col gap-2 ${msg.sender === 'user' ? 'items-end' : 'items-start'}`}>
                  <div className={`px-5 py-3.5 rounded-2xl shadow-xl ${
                    msg.sender === 'user'
                      ? 'bg-gradient-to-r from-indigo-500 to-purple-600 text-white rounded-tr-sm'
                      : 'glass-panel text-slate-200 rounded-tl-sm'
                  }`}>
                    <p className="text-sm leading-relaxed whitespace-pre-wrap">{msg.text}</p>
                  </div>
                  
                  {/* Sources (if AI message) */}
                  {msg.sources && msg.sources.length > 0 && (
                    <div className="mt-2 flex flex-col gap-2 w-full max-w-md">
                      <p className="text-xs font-semibold text-slate-400 uppercase tracking-wider ml-1">Sources</p>
                      <div className="flex flex-col gap-2">
                        {msg.sources.map((src, idx) => (
                          <div key={idx} className="glass-panel bg-slate-900/60 p-3 rounded-xl border border-white/5 hover:border-indigo-500/20 transition-colors">
                            <div className="flex items-center gap-2 mb-1.5">
                              <FileText className="w-3.5 h-3.5 text-indigo-400" />
                              <span className="text-xs font-medium text-indigo-300">{src.sourceFile}</span>
                              <span className="text-[10px] bg-white/5 px-2 py-0.5 rounded-full text-slate-400 border border-white/5">Chunk {src.sequenceIndex}</span>
                            </div>
                            <p className="text-xs text-slate-400 line-clamp-2 leading-relaxed bg-black/20 p-2 rounded-lg">{src.content}</p>
                          </div>
                        ))}
                      </div>
                    </div>
                  )}
                </div>
              </div>
            ))}
            
            {/* Thinking Indicator */}
            {isSearching && (
              <div className="flex gap-4">
                <div className="shrink-0 w-10 h-10 rounded-full bg-slate-800 border border-white/10 flex items-center justify-center shadow-lg">
                  <Bot className="w-5 h-5 text-indigo-300" />
                </div>
                <div className="glass-panel px-5 py-4 rounded-2xl rounded-tl-sm flex items-center gap-3">
                  <div className="flex gap-1.5">
                    <div className="w-2 h-2 rounded-full bg-indigo-400 animate-bounce" style={{ animationDelay: '0ms' }}></div>
                    <div className="w-2 h-2 rounded-full bg-indigo-400 animate-bounce" style={{ animationDelay: '150ms' }}></div>
                    <div className="w-2 h-2 rounded-full bg-indigo-400 animate-bounce" style={{ animationDelay: '300ms' }}></div>
                  </div>
                  <span className="text-sm font-medium text-indigo-300 animate-pulse">Analyzing knowledge base...</span>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          {/* Input Area */}
          <div className="p-6 bg-slate-900/40 backdrop-blur-md border-t border-white/10">
            <div className="relative flex items-center group">
              <input
                type="text"
                className="w-full glass-input rounded-2xl py-4 pl-5 pr-14 text-sm text-white placeholder-slate-400 outline-none shadow-inner"
                placeholder="Ask about your documents..."
                value={inputValue}
                onChange={(e) => setInputValue(e.target.value)}
                onKeyDown={handleKeyDown}
                disabled={isSearching}
              />
              <button
                onClick={handleSendMessage}
                disabled={!inputValue.trim() || isSearching}
                className="absolute right-2 p-2 rounded-xl bg-indigo-500/10 text-indigo-400 hover:bg-indigo-500 hover:text-white disabled:opacity-50 disabled:hover:bg-indigo-500/10 disabled:hover:text-indigo-400 transition-all duration-300 glass-button"
              >
                <Send className="w-5 h-5" />
              </button>
            </div>
            <p className="text-center text-[10px] text-slate-500 mt-3">
              Enterprise RAG uses advanced vector search to retrieve information from your secure documents.
            </p>
          </div>
        </div>

      </div>
    </div>
  );
}
