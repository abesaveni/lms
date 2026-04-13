import React, { useState } from 'react';
import { fraudDetection } from '../../services/aiApi';
import Button from '../ui/Button';
import { Card } from '../ui/Card';
import { ShieldAlert, Sparkles, AlertCircle, Fingerprint, Terminal } from 'lucide-react';
import { AIMarkdown } from '../ui/AIMarkdown';

export const FraudDetectionTool: React.FC = () => {
    const [transactionData, setTransactionData] = useState('');
    const [userIp, setUserIp] = useState('');
    const [loading, setLoading] = useState(false);
    const [response, setResponse] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        setError(null);
        setResponse(null);

        try {
            const result = await fraudDetection({ transactionData, userIp });
            setResponse(result.assessment);
        } catch (err: any) {
            setError(err.message || 'Failed to analyze fraud risk');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card className="p-6 border-0 shadow-xl bg-white/50 backdrop-blur-md">
            <div className="flex items-center justify-between mb-6">
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-slate-100 rounded-lg">
                        <ShieldAlert className="w-5 h-5 text-slate-700" />
                    </div>
                    <div>
                        <h3 className="text-xl font-bold text-gray-900">Fraud Scanner</h3>
                        <p className="text-xs text-gray-500 font-medium uppercase tracking-wider">Security Engine</p>
                    </div>
                </div>
                <div className="flex items-center gap-1.5 bg-slate-100 text-slate-700 px-3 py-1 rounded-full text-[10px] font-bold border border-slate-200">
                   <Sparkles className="w-3 h-3" />
                   ADMIN AI
                </div>
            </div>

            <form onSubmit={handleSubmit} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div>
                        <label className="block text-xs font-bold text-gray-500 uppercase mb-2 ml-1">User IP Address</label>
                        <input
                            type="text"
                            value={userIp}
                            onChange={(e) => setUserIp(e.target.value)}
                            className="w-full px-4 py-2.5 bg-gray-50 border border-gray-100 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-500 text-sm transition-all focus:bg-white"
                            placeholder="e.g. 192.168.1.1"
                            required
                        />
                    </div>
                    <div className="flex items-end justify-start pb-2">
                        <div className="flex items-center gap-2 text-[10px] text-gray-400 font-medium bg-gray-50 px-3 py-2 rounded-lg border border-gray-100 italic">
                           <Fingerprint className="w-3 h-3 text-gray-300" />
                           Scans for geolocation anomalies
                        </div>
                    </div>
                </div>
                <div>
                    <label className="block text-xs font-bold text-gray-500 uppercase mb-2 ml-1">Transaction / Behavior Logs</label>
                    <textarea
                        value={transactionData}
                        onChange={(e) => setTransactionData(e.target.value)}
                        className="w-full px-4 py-3 bg-gray-50 border border-gray-100 rounded-xl focus:outline-none focus:ring-2 focus:ring-slate-500 min-h-[140px] text-sm transition-all focus:bg-white"
                        placeholder="Paste payment attempt logs or login behavior data..."
                        required
                    />
                </div>
                <Button 
                    type="submit" 
                    isLoading={loading} 
                    className="w-full bg-slate-900 hover:bg-black text-white py-6 rounded-xl shadow-lg transition-all font-bold"
                >
                    Run AI Security Audit
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
                        <Terminal className="w-4 h-4 text-slate-500" />
                        <h4 className="text-sm font-bold text-gray-900">Security Assessment</h4>
                    </div>
                    <div className="p-5 bg-slate-900 rounded-2xl border border-slate-800 shadow-inner max-h-[250px] overflow-y-auto">
                        <div className="flex items-center gap-1.5 text-[8px] text-slate-500 font-bold mb-3 uppercase border-b border-slate-800 pb-2">
                            <span className="w-2 h-2 rounded-full bg-red-500 animate-pulse" />
                            Security Analysis Pulse
                        </div>
                        <AIMarkdown text={response} variant="dark" />
                    </div>
                </div>
            )}
        </Card>
    );
};
