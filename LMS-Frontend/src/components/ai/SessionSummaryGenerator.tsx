import React, { useState } from 'react';
import { generateSessionSummary } from '../../services/aiApi';
import Button from '../ui/Button';
import { Card } from '../ui/Card';
import { AIMarkdown } from '../ui/AIMarkdown';

export const SessionSummaryGenerator: React.FC = () => {
    const [transcript, setTranscript] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await generateSessionSummary({ transcript });
            setResponse(result.summary);
        } catch (err: any) {
            setError(err.message || 'Failed to generate session summary');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6">
            <h3 className="text-xl font-bold mb-4">AI Session Summary Generator</h3>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Session Transcript</label>
                    <textarea
                        value={transcript}
                        onChange={(e) => setTranscript(e.target.value)}
                        placeholder="Paste the raw session text here..."
                        rows={6}
                        required
                        className="w-full px-3 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                    />
                </div>
                <Button type="submit" isLoading={loading} className="w-full">
                    Generate Summary
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md border border-red-200">
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-6">
                    <h4 className="font-semibold mb-2">Session Summary:</h4>
                    <div className="p-4 bg-green-50 rounded-lg border border-green-100">
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
