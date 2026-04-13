import React, { useState } from 'react';
import { generateFlashcards } from '../../services/aiApi';
import Button from '../ui/Button';
import Input from '../ui/Input';
import { Card } from '../ui/Card';
import { AIMarkdown } from '../ui/AIMarkdown';

export const FlashcardsGenerator: React.FC = () => {
    const [topic, setTopic] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await generateFlashcards({ topic });
            setResponse(result.flashcards);
        } catch (err: any) {
            setError(err.message || 'Failed to generate flashcards');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6">
            <h3 className="text-xl font-bold mb-4">AI Flashcards Generator</h3>
            <p className="text-gray-600 mb-4 text-sm">Automatically generate key concepts and terms for any study topic.</p>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Topic / Subject Area</label>
                    <Input
                        value={topic}
                        onChange={(e) => setTopic(e.target.value)}
                        placeholder="e.g. Mitochondria, React Hooks, World War II..."
                        required
                    />
                </div>
                <Button type="submit" isLoading={loading} className="w-full">
                    Generate Flashcards
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md border border-red-200">
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-6">
                    <h4 className="font-semibold mb-2">Generated Flashcards:</h4>
                    <div className="p-4 bg-green-50 rounded-lg border border-green-100">
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
