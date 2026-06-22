# 🧠 Enterprise Document Intelligence: RAG Pipeline

A robust, full-stack **Retrieval-Augmented Generation (RAG)** system built to ingest, vectorize, and intelligently query enterprise documents (PDFs). 

This application bridges the gap between raw, private enterprise data and Large Language Models (LLMs), featuring a highly optimized Node.js / Express backend, a lightning-fast Groq Llama 3.1 inference engine, and a stunning React-based glassmorphism UI.

![Enterprise RAG UI](https://img.shields.io/badge/UI-React_|_Tailwind_CSS-blue)
![Backend](https://img.shields.io/badge/Backend-Node.js_|_Express-339933)
![Database](https://img.shields.io/badge/Vector_DB-MongoDB_Atlas-47A248)
![AI Models](https://img.shields.io/badge/AI-HuggingFace_|_Groq_Llama_3.1-F58025)

---

## ✨ Key Features

### 📄 Intelligent Document Ingestion
- **PDF Extraction**: Extracts raw text from uploaded PDF documents using `pdf-parse`.
- **Automated Chunking**: Splits large documents into token-optimized text chunks.
- **Idempotent Pipeline**: Automatically scrubs existing chunks for a document before re-ingestion to prevent database duplication.

### 🧠 Advanced Vectorization & Retrieval
- **HuggingFace Embeddings**: Generates 384-dimensional dense vectors using the free `sentence-transformers/all-MiniLM-L6-v2` model via raw HTTP client integration.
- **MongoDB Atlas Vector Search**: Uses Cosine Similarity to instantly retrieve the most semantically relevant text chunks.
- **Dynamic Context Filtering**: UI tracks the "Active Document" and injects strict filters into the `$vectorSearch` pipeline so the LLM only references the currently viewed file.

### 💬 Ultra-Fast LLM Inference
- **Powered by Groq**: Uses the `llama-3.1-8b-instant` model for lightning-fast token generation.
- **Context-Aware Prompts**: Injects the retrieved chunks directly into the system prompt to guarantee fact-based, hallucination-free answers.

### 🎨 Premium User Interface
- **Dark Glassmorphism**: A stunning, animated UI built with Vite, React, and Tailwind CSS.
- **Real-Time Feedback**: Animated upload spinners, typing indicators, and seamless chat integrations.

---

## 🛠️ Technology Stack

| Component | Technology |
| :--- | :--- |
| **Backend Framework** | Node.js, Express, TypeScript |
| **Frontend Framework** | React (Vite, TypeScript, Tailwind CSS, Lucide Icons) |
| **Vector Database** | MongoDB Atlas Vector Search |
| **Embedding Model** | HuggingFace Inference API (`all-MiniLM-L6-v2`) |
| **LLM Inference** | Groq API (`llama-3.1-8b-instant`) |

---

## 🚀 Getting Started

### 1. Prerequisites
- [Node.js](https://nodejs.org/en/)
- MongoDB Atlas Cluster (with a properly configured Vector Search Index)
- Groq API Key & HuggingFace API Token

### 2. Configure Environment Secrets
Create a `.env` file in the `enterprise-rag-node-api` directory with your credentials:
```env
MONGODB_CONNECTION_STRING="YOUR_MONGODB_CONNECTION_STRING"
MONGODB_DATABASE_NAME="EnterpriseRagDb"
MONGODB_COLLECTION_NAME="DocumentChunks"
GROQ_API_KEY="YOUR_GROQ_API_KEY"
HUGGINGFACE_API_TOKEN="YOUR_HUGGINGFACE_API_TOKEN"
PORT=5209
```

### 3. Start the Backend API
```bash
cd enterprise-rag-node-api
npm install
npm start
```
The API will start on `http://localhost:5209`.

### 4. Start the Frontend UI
Open a new terminal window:
```bash
cd enterprise-rag-ui
npm install
npm run dev
```
Navigate to `http://localhost:5173` in your browser to experience the application!

---

## 🏗️ Architecture Diagram (Data Flow)

1. **Upload**: User uploads PDF via React UI `->` `Node.js API (/upload)`.
2. **Process**: Text Extracted `->` Chunked `->` Sent to HuggingFace `->` 384D Vectors Generated.
3. **Store**: Vectors & Metadata bulk-inserted into `MongoDB Atlas`.
4. **Query**: User asks question `->` Question Vectorized via HuggingFace `->` `$vectorSearch` hits MongoDB.
5. **Answer**: Top 3 Context Chunks + Question sent to Groq `->` Llama 3.1 streams answer back to UI.
