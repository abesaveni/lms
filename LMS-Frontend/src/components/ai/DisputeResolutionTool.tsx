import React, { useState } from 'react';
import { disputeResolution } from '../../services/aiApi';
import Button from '../ui/Button';
import { Card } from '../ui/Card';
import { Scale, Sparkles, AlertCircle, CheckCircle2 } from 'lucide-react';
import { AIMarkdown } from '../ui/AIMarkdown';

export const DisputeResolutionTool: React.FC = () => {
    const [disputeDetails, setDisputeDetails] = useState('');
    const [sessionTranscript, setSessionTranscript] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await disputeResolution({ disputeDetails, sessionTranscript });
            setResponse(result.resolution);
        } catch (err: any) {
            setError(err.message || 'Failed to analyze dispute');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6 border-0 shadow-xl bg-white/50 backdrop-blur-md">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-purple-100 rounded-lg">
                        <Scale className="w-5 h-5 text-purple-600" />
                    </div>
                    <div>
                        <h3 className="text-xl font-bold text-gray-900">Dispute Mediator</h3>
                        <p className="text-xs text-gray-500 font-medium uppercase tracking-wider">AI Resolution</p>
                    </div>
                </div>
                <div className="flex items-center gap-1.5 bg-purple-50 text-purple-700 px-3 py-1 rounded-full text-[10px] font-bold border border-purple-100">
                   <Sparkles className="w-3 h-3" />
                   ADMIN AI
                </div>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-xs font-bold text-gray-500 uppercase mb-2 ml-1">Complaint Details</label>
                    <textarea
                        value={disputeDetails}
                        onChange={(e) => setDisputeDetails(e.target.value)}
                        className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-xl focus:outline-none focus:ring-2 focus:ring-purple-500 min-h-[100px] text-sm transition-all focus:bg-white"
                        placeholder="What is the nature of the dispute?"
                        required
                    />
                </div>
                <div>
                    <label className="block text-xs font-bold text-gray-500 uppercase mb-2 ml-1">Session Evidence (Transcript)</label>
                    <textarea
                        value={sessionTranscript}
                        onChange={(e) => setSessionTranscript(e.target.value)}
                        className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-xl focus:outline-none focus:ring-2 focus:ring-purple-500 min-h-[120px] text-sm transition-all focus:bg-white"
                        placeholder="Paste the full session transcript or chat logs for context..."
                        required
                    />
                </div>
                <Button 
                    type="submit" 
                    isLoading={loading} 
                    className="w-full bg-emerald-600 hover:bg-emerald-700 text-white py-6 rounded-xl shadow-lg shadow-emerald-200 transition-all font-bold"
                >
                    Mediate Dispute
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
                        <CheckCircle2 className="w-4 h-4 text-emerald-500" />
                        <h4 className="text-sm font-bold text-gray-900">Mediation Recommendation</h4>
                    </div>
                    <div className="p-5 bg-emerald-50/50 rounded-2xl border border-emerald-100 text-emerald-900 shadow-inner">
                        <div className="flex items-center gap-1.5 text-[8px] text-emerald-600 font-bold mb-3 uppercase border-b border-emerald-100 pb-2">
                            <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
                            AI Resolution Finalized
                        </div>
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
