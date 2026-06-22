import { Request, Response } from 'express';
import { extractTextFromPdf } from '../services/pdfService';
import { generateChunks } from '../services/chunkingService';
import { generateEmbedding } from '../services/embeddingService';
import { getDocumentChunksCollection } from '../config/db';

export const uploadFile = async (req: Request, res: Response): Promise<void> => {
  try {
    const file = req.file;
    if (!file) {
      res.status(400).json({ error: "No file was uploaded." });
      return;
    }

    if (file.mimetype !== 'application/pdf') {
      res.status(400).json({ error: "Only PDF files are supported." });
      return;
    }

    const fileName = file.originalname;
    console.log(`Starting pipeline upload for file: ${fileName}`);

    const rawText = await extractTextFromPdf(file.buffer);
    if (!rawText.trim()) {
      res.status(400).json({ error: "No extractable text found in the PDF." });
      return;
    }

    const chunks = generateChunks(rawText);
    if (chunks.length === 0) {
      res.status(400).json({ error: "Failed to split document text into chunks." });
      return;
    }

    const documentChunks = [];
    for (let i = 0; i < chunks.length; i++) {
      const textChunk = chunks[i];
      const embedding = await generateEmbedding(textChunk);
      
      documentChunks.push({
        SourceFile: fileName,
        Content: textChunk,
        SequenceIndex: i,
        Embedding: embedding,
        CreatedAt: new Date()
      });
    }

    const collection = getDocumentChunksCollection();
    
    console.log(`Removing existing chunks for ${fileName}`);
    await collection.deleteMany({ SourceFile: fileName });

    console.log(`Bulk inserting ${documentChunks.length} chunks`);
    await collection.insertMany(documentChunks);

    res.status(200).json({
      fileName,
      totalChunksProcessed: documentChunks.length,
      status: "Successfully Persisted"
    });
  } catch (error: any) {
    console.error("Ingestion error:", error);
    res.status(500).json({ error: "An unexpected error occurred during pipeline processing.", details: error.message });
  }
};
