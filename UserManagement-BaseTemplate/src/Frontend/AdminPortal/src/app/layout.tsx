import * as React from 'react';
import '@/app/globals.css';

export const metadata = {
  title: 'base Admin Portal',
  description: 'Administrative interface for managing base data and users',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="antialiased">
        {children}
      </body>
    </html>
  );
}
