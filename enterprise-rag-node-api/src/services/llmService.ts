import Groq from 'groq-sdk';
import dotenv from 'dotenv';

dotenv.config();

const groq = new Groq({
    apiKey: process.env.GROQ_API_KEY
});

export async function generateAnswer(userQuestion: string, context: string): Promise<string> {
    const systemPrompt = `You are a helpful enterprise AI assistant. You must answer the user's question using strictly the provided context below. Do not hallucinate or use outside knowledge. If the answer is not in the context, say "I cannot answer this based on the provided document."\n\nContext:\n${context}`;

    try {
        const response = await groq.chat.completions.create({
            model: "llama-3.1-8b-instant",
            messages: [
                { role: "system", content: systemPrompt },
                { role: "user", content: userQuestion }
            ],
            temperature: 0,
            max_tokens: 1024
        });

        return response.choices[0]?.message?.content || "No response generated.";
    } catch (error) {
        console.error('Error generating LLM answer:', error);
        throw new Error('Failed to generate answer from Groq');
    }
}
