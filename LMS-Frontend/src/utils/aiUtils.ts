/**
 * Strips markdown code fences (```json ... ```) from AI responses before JSON.parse.
 * Backend AI responses sometimes wrap JSON in code fences which breaks parsing.
 */
export function stripCodeFences(text: string): string {
  return text
    .replace(/^```[\w]*\s*\n?/, '')
    .replace(/\n?```\s*$/, '')
    .trim()
}
