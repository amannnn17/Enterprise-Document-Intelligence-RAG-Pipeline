import { Request, Response } from 'express';
import { generateEmbedding } from '../services/embeddingService';
import { generateAnswer } from '../services/llmService';
import { getDocumentChunksCollection } from '../config/db';

export const search = async (req: Request, res: Response): Promise<void> => {
  try {
    const { userQuestion, documentName } = req.body;
    
    if (!userQuestion) {
      res.status(400).json({ error: "UserQuestion is required." });
      return;
    }

    console.log(`Received question: ${userQuestion} for document: ${documentName}`);

    const queryVector = await generateEmbedding(userQuestion);

    const collection = getDocumentChunksCollection();
    
    const vectorSearchStage: any = {
      $vectorSearch: {
        index: "vector_index",
        path: "Embedding",
        queryVector: queryVector,
        numCandidates: 100,
        limit: 3
      }
    };

    if (documentName) {
      vectorSearchStage.$vectorSearch.filter = { SourceFile: documentName };
    }

    const pipeline = [vectorSearchStage];
    const similarChunks = await collection.aggregate(pipeline).toArray();

    let context = "";
    const results = [];

    for (const chunk of similarChunks) {
      context += `[Document: ${chunk.SourceFile}, Chunk: ${chunk.SequenceIndex}]\n${chunk.Content}\n\n`;
      results.push({
        sourceFile: chunk.SourceFile,
        sequenceIndex: chunk.SequenceIndex,
        content: chunk.Content
      });
    }

    const answer = await generateAnswer(userQuestion, context);

    res.status(200).json({
      answer: answer,
      results: results
    });

  } catch (error: any) {
    console.error("Query error:", error);
    res.status(500).json({ error: "An unexpected error occurred during search.", details: error.message });
  }
};
