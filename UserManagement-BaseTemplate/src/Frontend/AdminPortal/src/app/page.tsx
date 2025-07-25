'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/lib/store/auth';

export default function HomePage() {
  const router = useRouter();
  const authStore = useAuthStore();
  const [isLoading, setIsLoading] = useState(true);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  useEffect(() => {
    // Load auth state from localStorage only after component is mounted
    if (mounted) {
      (authStore as any).loadUserFromStorage();
      setIsLoading(false);
    }
  }, [mounted, authStore]);

  useEffect(() => {
    // Only redirect after auth state is loaded
    if (!isLoading) {
      if (authStore.isAuthenticated && authStore.user) {
        // User is logged in, redirect to dashboard
        router.replace('/dashboard');
      } else {
        // User is not logged in, redirect to login
        router.replace('/login');
      }
    }
  }, [authStore.isAuthenticated, authStore.user, router, isLoading]);

  // Show loading while determining redirect
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
        <p className="mt-4 text-gray-600">Loading...</p>
      </div>
    </div>
  );
}
