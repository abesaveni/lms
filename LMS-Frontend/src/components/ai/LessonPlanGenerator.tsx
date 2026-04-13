import React, { useState } from 'react';
import { generateLessonPlan } from '../../services/aiApi';
import Button from '../ui/Button';
import Input from '../ui/Input';
import { Card } from '../ui/Card';
import { AIMarkdown } from '../ui/AIMarkdown';

export const LessonPlanGenerator: React.FC = () => {
    const [subject, setSubject] = useState('');
    const [level, setLevel] = useState('');
    const [topic, setTopic] = useState('');
    const [learningObjectives, setLearningObjectives] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await generateLessonPlan({ subject, level, topic, learningObjectives });
            setResponse(result.plan);
        } catch (err: any) {
            setError(err.message || 'Failed to generate lesson plan');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6">
            <h3 className="text-xl font-bold mb-4">AI Lesson Plan Generator</h3>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label className="block text-sm font-medium mb-1">Subject</label>
                        <Input
                            value={subject}
                            onChange={(e) => setSubject(e.target.value)}
                            placeholder="e.g. Biology"
                            required
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium mb-1">Level</label>
                        <Input
                            value={level}
                            onChange={(e) => setLevel(e.target.value)}
                            placeholder="e.g. Grade 10"
                            required
                        />
                    </div>
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">Topic</label>
                    <Input
                        value={topic}
                        onChange={(e) => setTopic(e.target.value)}
                        placeholder="e.g. Photosynthesis"
                        required
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">Learning Objectives</label>
                    <textarea
                        value={learningObjectives}
                        onChange={(e) => setLearningObjectives(e.target.value)}
                        className="w-full px-3 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 min-h-[100px]"
                        placeholder="What should the student learn?"
                        required
                    />
                </div>
                <Button type="submit" isLoading={loading} className="w-full">
                    Create Lesson Plan
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-3 bg-red-50 text-red-700 rounded-md border border-red-200">
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-6">
                    <h4 className="font-semibold mb-2">Lesson Plan:</h4>
                    <div className="p-4 bg-teal-50 rounded-lg border border-teal-100">
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
