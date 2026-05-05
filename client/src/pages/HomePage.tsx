import { useNavigate } from 'react-router';

export function HomePage() {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4">
      <h1 className="text-5xl font-bold text-white mb-2">Card Game</h1>
      <p className="text-slate-400 mb-12 text-lg">Play card games with friends online</p>

      <div className="flex flex-col gap-4 w-full max-w-xs">
        <button
          onClick={() => navigate('/create')}
          className="bg-indigo-600 hover:bg-indigo-500 text-white font-semibold py-4 px-8 rounded-xl text-lg transition-colors"
        >
          Create Room
        </button>
        <button
          onClick={() => navigate('/join')}
          className="bg-slate-700 hover:bg-slate-600 text-white font-semibold py-4 px-8 rounded-xl text-lg transition-colors"
        >
          Join Room
        </button>
      </div>
    </div>
  );
}
