# Enterprise Document Intelligence RAG Pipeline

A robust .NET Core Web API built to ingest, process, and vectorize enterprise documents (such as PDFs) to power a Retrieval-Augmented Generation (RAG) pipeline. 

This project aims to bridge the gap between raw enterprise data and Large Language Models (LLMs) by providing a scalable backend for document processing and similarity search.

## 🚀 Current Features

### Phase 1: Parsing & Chunking (Completed)
- **PDF Extraction**: Extracts raw text from uploaded PDF documents.
- **Intelligent Chunking**: Splits large documents into smaller, manageable text chunks based on token counts (handling spaces, line breaks, and tabs).
- **Debug Endpoint**: A dedicated `/api/v1/ingestion/debug-chunk` endpoint to test and visualize the chunking strategy before vectorization.

### Phase 2: Vectorization (In Progress)
- Converting text chunks into high-dimensional vector embeddings using an LLM.
- Storing vectors in a Vector Database for fast semantic retrieval.

## 🛠️ Tech Stack
- **Framework**: .NET Core 8 Web API
- **Language**: C#
- **Documentation**: Swagger / OpenAPI
- **Architecture**: Service-Oriented Architecture (Controllers -> Services)

## 💻 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Running the API Locally
1. Clone the repository and navigate to the project directory:
   ```bash
   cd EnterpriseRag.Api
   ```
2. Build and run the project:
   ```bash
   dotnet run
   ```
3. Open your browser and navigate to the Swagger UI:
   ```
   http://localhost:5209/swagger
   ```

### Testing the Ingestion Pipeline
You can test the parsing and chunking logic directly from the Swagger UI:
1. Go to the `POST /api/v1/ingestion/debug-chunk` endpoint.
2. Click **Try it out**.
3. Upload a sample `.pdf` file.
4. Click **Execute** and observe the JSON response containing your extracted and chunked text!

## 🛣️ Roadmap
- [x] Document Ingestion (PDF)
- [x] Token-based Chunking
- [ ] Embedding Generation (Vectorization)
- [ ] Vector Database Integration (Qdrant / Pinecone / Milvus)
- [ ] Query and Retrieval Endpoint
- [ ] Integration with an LLM for answering queries
