import React, { useState } from 'react';
import { generateProgressReport } from '../../services/aiApi';
import Button from '../ui/Button';
import Input from '../ui/Input';
import { Card } from '../ui/Card';
import { AIMarkdown } from '../ui/AIMarkdown';

export const ProgressReportGenerator: React.FC = () => {
    const [studentName, setStudentName] = useState('');
    const [feedbackHistory, setFeedbackHistory] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await generateProgressReport({ studentName, feedbackHistory });
            setResponse(result.report);
        } catch (err: any) {
            setError(err.message || 'Failed to generate progress report');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6">
            <h3 className="text-xl font-bold mb-4">AI Student Progress Report</h3>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Student Name</label>
                    <Input
                        value={studentName}
                        onChange={(e) => setStudentName(e.target.value)}
                        placeholder="e.g. Charlie Brown"
                        required
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">Feedback & Session History</label>
                    <textarea
                        value={feedbackHistory}
                        onChange={(e) => setFeedbackHistory(e.target.value)}
                        className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 min-h-[150px]"
                        placeholder="Summarize recent feedback, scores, or session notes for this student..."
                        required
                    />
                </div>
                <Button type="submit" isLoading={loading} className="w-full">
                    Generate Progress Report
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md border border-red-200">
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-6">
                    <h4 className="font-semibold mb-2">Progress Report Analysis:</h4>
                    <div className="p-4 bg-indigo-50 rounded-lg border border-indigo-100">
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
