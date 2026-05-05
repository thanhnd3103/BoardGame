import type { ReactNode } from 'react';

export function PageContainer({ children, full }: { children: ReactNode; full?: boolean }) {
  return (
    <div className={`flex flex-col items-center justify-center ${full ? 'min-h-screen' : 'min-h-screen'} p-4`}>
      <div className={full ? 'w-full h-full' : 'w-full max-w-lg'}>
        {children}
      </div>
    </div>
  );
}
