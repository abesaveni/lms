import React, { useState } from 'react';
import { supportTriage } from '../../services/aiApi';
import Button from '../ui/Button';
import { Card } from '../ui/Card';
import { BadgeCheck, Sparkles, MessageSquare, AlertCircle } from 'lucide-react';
import { AIMarkdown } from '../ui/AIMarkdown';

export const SupportTriageDashboard: React.FC = () => {
    const [ticketContent, setTicketContent] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await supportTriage({ ticketContent });
            setResponse(result.triage);
        } catch (err: any) {
            setError(err.message || 'Failed to triage ticket');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6 border-0 shadow-xl bg-white/50 backdrop-blur-md">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-blue-100 rounded-lg">
                        <MessageSquare className="w-5 h-5 text-blue-600" />
                    </div>
                    <div>
                        <h3 className="text-xl font-bold text-gray-900">Support Triage</h3>
                        <p className="text-xs text-gray-500 font-medium uppercase tracking-wider">Automated Priority</p>
                    </div>
                </div>
                <div className="flex items-center gap-1.5 bg-blue-50 text-blue-700 px-3 py-1 rounded-full text-[10px] font-bold border border-blue-100">
                   <Sparkles className="w-3 h-3" />
                   ADMIN AI
                </div>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5">
                <div>
                    <label className="block text-xs font-bold text-gray-500 uppercase mb-2 ml-1">Ticket / Query Text</label>
                    <textarea
                        value={ticketContent}
                        onChange={(e) => setTicketContent(e.target.value)}
                        className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-xl focus:outline-none focus:ring-2 focus:ring-blue-500 min-h-[140px] text-sm transition-all focus:bg-white"
                        placeholder="Paste the support request or complaint details..."
                        required
                    />
                </div>
                <Button 
                    type="submit" 
                    isLoading={loading} 
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white py-6 rounded-xl shadow-lg shadow-blue-200 transition-all font-bold"
                >
                    Categorize & Prioritize
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
                        <BadgeCheck className="w-4 h-4 text-blue-500" />
                        <h4 className="text-sm font-bold text-gray-900">Triage Report</h4>
                    </div>
                    <div className="p-5 bg-blue-50/50 rounded-2xl border border-blue-100 text-blue-900 shadow-inner">
                        <div className="flex items-center gap-1.5 text-[8px] text-blue-600 font-bold mb-3 uppercase border-b border-blue-100 pb-2">
                            <span className="w-2 h-2 rounded-full bg-blue-500 animate-pulse" />
                            AI Triage Complete
                        </div>
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
