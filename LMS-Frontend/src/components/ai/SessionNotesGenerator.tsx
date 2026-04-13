import React, { useState } from 'react';
import { generateSessionNotes } from '../../services/aiApi';
import Button from '../ui/Button';
import { Card } from '../ui/Card';
import { AIMarkdown } from '../ui/AIMarkdown';

export const SessionNotesGenerator: React.FC = () => {
    const [transcript, setTranscript] = useState('');
    const [focusAreas, setFocusAreas] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await generateSessionNotes({ transcript, focusAreas });
            setResponse(result.notes);
        } catch (err: any) {
            setError(err.message || 'Failed to generate session notes');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6">
            <h3 className="text-xl font-bold mb-4">AI Session Notes Generator</h3>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Session Transcript</label>
                    <textarea
                        value={transcript}
                        onChange={(e) => setTranscript(e.target.value)}
                        className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 min-h-[150px]"
                        placeholder="Paste session transcript or chat history here..."
                        required
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">Specific Focus Areas</label>
                    <input
                        type="text"
                        value={focusAreas}
                        onChange={(e) => setFocusAreas(e.target.value)}
                        className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="e.g. Problem solving, Pronunciation, Conceptual understanding"
                    />
                </div>
                <Button type="submit" isLoading={loading} className="w-full">
                    Generate Professional Notes
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md border border-red-200">
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-6">
                    <h4 className="font-semibold mb-2">Tutor Session Notes:</h4>
                    <div className="p-4 bg-orange-50 rounded-lg border border-orange-100">
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
