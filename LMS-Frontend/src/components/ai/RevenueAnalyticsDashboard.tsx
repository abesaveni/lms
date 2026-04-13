import React, { useState } from 'react';
import { revenueAnalytics } from '../../services/aiApi';
import Button from '../ui/Button';
import { Card } from '../ui/Card';
import { BarChart3, Sparkles, TrendingUp, AlertCircle } from 'lucide-react';
import { AIMarkdown } from '../ui/AIMarkdown';

export const RevenueAnalyticsDashboard: React.FC = () => {
    const [financialData, setFinancialData] = useState('');
    const [period, setPeriod] = useState('monthly');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await revenueAnalytics({ financialData, period });
            setResponse(result.analytics);
        } catch (err: any) {
            setError(err.message || 'Failed to analyze revenue');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6 border-0 shadow-xl bg-white/50 backdrop-blur-md">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-emerald-100 rounded-lg">
                        <TrendingUp className="w-5 h-5 text-emerald-600" />
                    </div>
                    <div>
                        <h3 className="text-xl font-bold text-gray-900">Revenue Forecasting</h3>
                        <p className="text-xs text-gray-500 font-medium uppercase tracking-wider">Financial Intelligence</p>
                    </div>
                </div>
                <div className="flex items-center gap-1.5 bg-emerald-50 text-emerald-700 px-3 py-1 rounded-full text-[10px] font-bold border border-emerald-100">
                   <Sparkles className="w-3 h-3" />
                   ADMIN AI
                </div>
            </div>

            <form onSubmit={handleSubmit} className="space-y-5">
                <div className="flex p-1 bg-gray-100 rounded-xl w-fit">
                    <button
                        type="button"
                        onClick={() => setPeriod('monthly')}
                        className={`px-4 py-1.5 rounded-lg text-xs font-bold transition-all ${
                            period === 'monthly' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'
                        }`}
                    >
                        Monthly
                    </button>
                    <button
                        type="button"
                        onClick={() => setPeriod('quarterly')}
                        className={`px-4 py-1.5 rounded-lg text-xs font-bold transition-all ${
                            period === 'quarterly' ? 'bg-white text-gray-900 shadow-sm' : 'text-gray-500 hover:text-gray-700'
                        }`}
                    >
                        Quarterly
                    </button>
                </div>
                <div>
                    <label className="block text-xs font-bold text-gray-500 uppercase mb-2 ml-1">Financial Data Stream</label>
                    <textarea
                        value={financialData}
                        onChange={(e) => setFinancialData(e.target.value)}
                        className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-500 min-h-[140px] text-sm transition-all focus:bg-white"
                        placeholder="Paste payout summaries, fee totals, and revenue logs..."
                        required
                    />
                </div>
                <Button 
                    type="submit" 
                    isLoading={loading} 
                    className="w-full bg-slate-900 hover:bg-black text-white py-6 rounded-xl shadow-lg transition-all font-bold"
                >
                    Run Revenue Analytics
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
                        <BarChart3 className="w-4 h-4 text-emerald-500" />
                        <h4 className="text-sm font-bold text-gray-900">Forecasting Results</h4>
                    </div>
                    <div className="p-5 bg-emerald-50/50 rounded-2xl border border-emerald-100 text-emerald-900 shadow-inner">
                        <div className="flex items-center gap-1.5 text-[8px] text-emerald-600 font-bold mb-3 uppercase border-b border-emerald-100 pb-2">
                            <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
                            AI Insight Ready
                        </div>
                        <AIMarkdown text={response} />
                    </div>
                </div>
            )}
        </Card>
    );
};
