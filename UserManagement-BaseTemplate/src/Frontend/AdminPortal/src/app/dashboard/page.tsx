'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '@/lib/store/auth';
import Link from 'next/link';

export default function DashboardPage(): React.JSX.Element {
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
    // Redirect to login if not authenticated
    if (!isLoading && (!authStore.isAuthenticated || !authStore.user)) {
      router.replace('/login');
    }
  }, [isLoading, authStore.isAuthenticated, authStore.user, router]);

  const handleLogout = () => {
    authStore.logout();
    router.replace('/login');
  };

  // Show loading while checking auth
  if (isLoading || !authStore.isAuthenticated || !authStore.user) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center py-6">
            <h1 className="text-3xl font-bold text-gray-900">
              Admin Dashboard
            </h1>
            <div className="flex items-center space-x-4">
              <button onClick={handleLogout} className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition">Đăng xuất</button>
            </div>
          </div>
        </div>
      </div>
      {/* Main content */}
      <div className="max-w-7xl mx-auto py-10 px-4 sm:px-6 lg:px-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
          {/* Example card */}
          <div className="bg-white rounded-xl shadow p-6 flex flex-col items-center justify-center">
            <h2 className="text-xl font-semibold text-gray-800 mb-2">Quản lý người dùng</h2>
            <p className="text-gray-500 mb-4">Thêm, sửa, xóa tài khoản người dùng.</p>
            <a href="/dashboard/user-manager" className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition">Đi tới quản lý</a>
          </div>
          {/* Thêm các card khác nếu cần */}
        </div>
      </div>
    </div>
  );
}
