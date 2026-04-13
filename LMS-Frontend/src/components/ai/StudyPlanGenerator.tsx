import React, { useState } from 'react';
import { generateStudyPlan } from '../../services/aiApi';
import Button from '../ui/Button';
import Input from '../ui/Input';
import { Card } from '../ui/Card';
import { AIMarkdown } from '../ui/AIMarkdown';

export const StudyPlanGenerator: React.FC = () => {
    const [subject, setSubject] = useState('');
    const [goal, setGoal] = useState('');
    const [time, setTime] = useState('');
    const [duration, setDuration] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await generateStudyPlan({ subject, goal, time, duration });
            setResponse(result.plan);
        } catch (err: any) {
            setError(err.message || 'Failed to generate study plan');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6">
            <h3 className="text-xl font-bold mb-4">AI Study Plan Generator</h3>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-medium mb-1">Subject</label>
                        <Input
                            value={subject}
                            onChange={(e) => setSubject(e.target.value)}
                            placeholder="e.g. Physics"
                            required
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium mb-1">Goal</label>
                        <Input
                            value={goal}
                            onChange={(e) => setGoal(e.target.value)}
                            placeholder="e.g. Prepare for finals"
                            required
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium mb-1">Time per day</label>
                        <Input
                            value={time}
                            onChange={(e) => setTime(e.target.value)}
                            placeholder="e.g. 2 hours"
                            required
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium mb-1">Total Duration</label>
                        <Input
                            value={duration}
                            onChange={(e) => setDuration(e.target.value)}
                            placeholder="e.g. 4 weeks"
                            required
                        />
                    </div>
                </div>
                <Button type="submit" isLoading={loading} className="w-full">
                    Generate Study Plan
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md border border-red-200">
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-6">
                    <h4 className="font-semibold mb-2">Weekly Study Plan:</h4>
                    <div className="p-4 bg-green-50 rounded-lg border border-green-100">
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
