import React, { useState } from 'react';
import { generateQuiz } from '../../services/aiApi';
import Button from '../ui/Button';
import Input from '../ui/Input';
import { Card } from '../ui/Card';
import { AIMarkdown } from '../ui/AIMarkdown';

export const QuizGenerator: React.FC = () => {
    const [topic, setTopic] = useState('');
    const [difficulty, setDifficulty] = useState('Intermediate');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await generateQuiz({ topic, difficulty });
            setResponse(result.quiz);
        } catch (err: any) {
            setError(err.message || 'Failed to generate quiz');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6">
            <h3 className="text-xl font-bold mb-4">AI Quiz Generator</h3>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Topic</label>
                    <Input
                        value={topic}
                        onChange={(e) => setTopic(e.target.value)}
                        placeholder="e.g. Ancient History, Calculus"
                        required
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">Difficulty</label>
                    <select
                        value={difficulty}
                        onChange={(e) => setDifficulty(e.target.value)}
                        className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
                    >
                        <option value="Beginner">Beginner</option>
                        <option value="Intermediate">Intermediate</option>
                        <option value="Advanced">Advanced</option>
                    </select>
                </div>
                <Button type="submit" isLoading={loading} className="w-full">
                    Generate 10 Questions
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md border border-red-200">
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-6">
                    <h4 className="font-semibold mb-2">Quiz Questions:</h4>
                    <div className="p-4 bg-purple-50 rounded-lg border border-purple-100">
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
