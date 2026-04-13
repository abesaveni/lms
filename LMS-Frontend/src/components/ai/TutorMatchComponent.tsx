import React, { useState } from 'react';
import { tutorMatch } from '../../services/aiApi';
import Button from '../ui/Button';
import Input from '../ui/Input';
import { Card } from '../ui/Card';
import { AIMarkdown } from '../ui/AIMarkdown';

export const TutorMatchComponent: React.FC = () => {
    const [subject, setSubject] = useState('');
    const [level, setLevel] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await tutorMatch({ subject, level });
            setResponse(result.recommendation);
        } catch (err: any) {
            setError(err.message || 'Failed to match tutor');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6">
            <h3 className="text-xl font-bold mb-4">AI Tutor Match</h3>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Subject</label>
                    <Input
                        value={subject}
                        onChange={(e) => setSubject(e.target.value)}
                        placeholder="e.g. Mathematics, Organic Chemistry"
                        required
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">Level</label>
                    <Input
                        value={level}
                        onChange={(e) => setLevel(e.target.value)}
                        placeholder="e.g. University, High School"
                        required
                    />
                </div>
                <Button type="submit" isLoading={loading} className="w-full">
                    Find Best Match
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md border border-red-200">
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-6">
                    <h4 className="font-semibold mb-2">Recommendation:</h4>
                    <div className="p-4 bg-blue-50 rounded-lg border border-blue-100">
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
