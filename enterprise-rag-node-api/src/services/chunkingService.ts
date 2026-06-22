export function generateChunks(text: string, maxTokens: number = 500, overlapTokens: number = 50): string[] {
  const delimiters = /[\s\r\n\t]+/;
  const words = text.split(delimiters).filter(word => word.length > 0);
  
  const chunks: string[] = [];
  let currentIndex = 0;

  while (currentIndex < words.length) {
    let chunkSize = Math.min(maxTokens, words.length - currentIndex);
    const chunkWords = words.slice(currentIndex, currentIndex + chunkSize);
    
    chunks.push(chunkWords.join(' '));
    
    if (currentIndex + chunkSize >= words.length) {
      break;
    }
    
    currentIndex += (maxTokens - overlapTokens);
  }
  
  return chunks;
}
