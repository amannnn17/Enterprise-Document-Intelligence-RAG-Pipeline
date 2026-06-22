import express from 'express';
import cors from 'cors';
import multer from 'multer';
import { connectDB } from './config/db';
import { uploadFile } from './controllers/ingestionController';
import { search } from './controllers/queryController';

const app = express();
const port = process.env.PORT || 5209;

app.use(cors());
app.use(express.json());

const upload = multer({ storage: multer.memoryStorage() });

app.post('/api/v1/ingestion/upload', upload.single('file'), uploadFile);
app.post('/api/v1/query/search', search);

connectDB().then(() => {
  app.listen(port, () => {
    console.log(`Node.js Enterprise RAG Backend listening at http://localhost:${port}`);
  });
});
