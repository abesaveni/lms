import React, { useState } from 'react';
import { churnPrediction } from '../../services/aiApi';
import Button from '../ui/Button';
import { Card } from '../ui/Card';
import { AlertCircle, BrainCircuit, Sparkles, Terminal } from 'lucide-react';
import { AIMarkdown } from '../ui/AIMarkdown';

export const ChurnPredictionDashboard: React.FC = () => {
    const [studentUsageData, setStudentUsageData] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await churnPrediction({ studentUsageData });
            setResponse(result.prediction);
        } catch (err: any) {
            setError(err.message || 'Failed to predict churn');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6 border-0 shadow-xl bg-white/50 backdrop-blur-md">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-indigo-100 rounded-lg">
                        <BrainCircuit className="w-5 h-5 text-indigo-600" />
                    </div>
                    <div>
                        <h3 className="text-xl font-bold text-gray-900">Churn Analysis</h3>
                        <p className="text-xs text-gray-500 font-medium uppercase tracking-wider">Predictive Retrieval</p>
                    </div>
                </div>
                <div className="flex items-center gap-1.5 bg-indigo-50 text-indigo-700 px-3 py-1 rounded-full text-[10px] font-bold border border-indigo-100">
                   <Sparkles className="w-3 h-3" />
                   ADMIN AI
                </div>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5">
                <div>
                    <label className="block text-xs font-bold text-gray-500 uppercase mb-2 ml-1">Usage Data Summary</label>
                    <textarea
                        value={studentUsageData}
                        onChange={(e) => setStudentUsageData(e.target.value)}
                        className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500 min-h-[140px] text-sm transition-all focus:bg-white"
                        placeholder="Paste usage counts, login logs, or last active sessions..."
                        required
                    />
                </div>
                <Button 
                    type="submit" 
                    isLoading={loading} 
                    className="w-full bg-indigo-600 hover:bg-indigo-700 text-white py-6 rounded-xl shadow-lg shadow-indigo-200 transition-all font-bold"
                >
                    Generate Retention Insights
                </Button>
            </form>

            {error && (
                <div className="mt-4 p-4 bg-red-50 text-red-700 rounded-xl border border-red-100 flex items-center gap-3 text-sm">
                    <AlertCircle className="w-5 h-5 flex-shrink-0" />
                    {error}
                </div>
            )}

            {response && (
                <div className="mt-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
                    <div className="flex items-center gap-2 mb-3 px-1">
                        <Terminal className="w-4 h-4 text-gray-400" />
                        <h4 className="text-sm font-bold text-gray-900">Prediction Report</h4>
                    </div>
                    <div className="p-5 bg-slate-900 rounded-2xl border border-slate-800 shadow-inner max-h-[250px] overflow-y-auto">
                        <div className="flex items-center gap-1.5 text-[8px] text-slate-500 font-bold mb-3 uppercase border-b border-slate-800 pb-2">
                            <span className="w-2 h-2 rounded-full bg-red-500 animate-pulse" />
                            Live AI Analysis Stream
                        </div>
                        <AIMarkdown text={response} variant="dark" />
                    </div>
                </div>
            )}
        </Card>
    );
};
