import { HfInference } from '@huggingface/inference';
import dotenv from 'dotenv';

dotenv.config();

const hfToken = process.env.HUGGINGFACE_API_TOKEN;
const hf = new HfInference(hfToken);
const model = 'sentence-transformers/all-MiniLM-L6-v2';

export async function generateEmbedding(text: string): Promise<number[]> {
  try {
    const result = await hf.featureExtraction({
      model: model,
      inputs: text,
    });
    
    if (Array.isArray(result)) {
      if (Array.isArray(result[0])) {
         return result[0] as number[];
      }
      return result as number[];
    }
    
    throw new Error('Unexpected embedding format');
  } catch (error) {
    console.error('Error generating embedding:', error);
    throw new Error('Failed to generate embedding');
  }
}
