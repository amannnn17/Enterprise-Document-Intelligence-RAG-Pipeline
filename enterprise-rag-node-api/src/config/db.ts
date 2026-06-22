import { MongoClient, Collection } from 'mongodb';
import dotenv from 'dotenv';

dotenv.config();

const uri = process.env.MONGODB_CONNECTION_STRING || '';
const dbName = process.env.MONGODB_DATABASE_NAME || 'EnterpriseRagDb';
const collectionName = process.env.MONGODB_COLLECTION_NAME || 'DocumentChunks';

const client = new MongoClient(uri);

let documentChunksCollection: Collection;

export async function connectDB() {
  try {
    await client.connect();
    console.log('Connected successfully to MongoDB Atlas');
    const db = client.db(dbName);
    documentChunksCollection = db.collection(collectionName);
  } catch (error) {
    console.error('MongoDB connection error:', error);
    process.exit(1);
  }
}

export function getDocumentChunksCollection(): Collection {
  if (!documentChunksCollection) {
    throw new Error('Database not initialized');
  }
  return documentChunksCollection;
}
