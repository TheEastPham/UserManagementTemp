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
              <Link 
                href="/profile"
                className="text-indigo-600 hover:text-indigo-900 font-medium"
              >
                My Profile
              </Link>
              <span className="text-gray-500">|</span>
              <span className="text-gray-700">
                Welcome, {authStore.user.email}
              </span>
              <button
                onClick={handleLogout}
                className="bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-4 rounded"
              >
                Logout
              </button>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="border-4 border-dashed border-gray-200 rounded-lg p-6">
            <h1 className="text-3xl font-bold text-gray-900 mb-4">
              Welcome to Admin Portal
            </h1>
            <div className="mb-6">
              <p className="text-lg text-gray-700 mb-2">
                Hello, {authStore.user.email}! You are logged in as {authStore.user.role}.
              </p>
              <div className="bg-indigo-50 border border-indigo-200 rounded-md p-4 mb-4">
                <h3 className="text-lg font-medium text-indigo-900 mb-2">Quick Actions</h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <Link 
                    href="/profile"
                    className="block bg-white p-4 rounded-lg shadow hover:shadow-md transition-shadow"
                  >
                    <h4 className="font-semibold text-gray-900">Manage Profile</h4>
                    <p className="text-sm text-gray-600 mt-1">Update your personal information and settings</p>
                  </Link>
                  <div className="block bg-white p-4 rounded-lg shadow hover:shadow-md transition-shadow cursor-pointer">
                    <h4 className="font-semibold text-gray-900">User Management</h4>
                    <p className="text-sm text-gray-600 mt-1">Manage system users and permissions</p>
                  </div>
                  <div className="block bg-white p-4 rounded-lg shadow hover:shadow-md transition-shadow cursor-pointer">
                    <h4 className="font-semibold text-gray-900">System Settings</h4>
                    <p className="text-sm text-gray-600 mt-1">Configure system-wide settings</p>
                  </div>
                </div>
              </div>
            </div>
            <div className="bg-yellow-50 border border-yellow-200 rounded-md p-4">
              <h3 className="text-lg font-medium text-yellow-900 mb-2">Recent Activity</h3>
              <ul className="space-y-2">
                <li className="text-sm text-yellow-800">• System backup completed successfully</li>
                <li className="text-sm text-yellow-800">• 3 new user registrations pending approval</li>
                <li className="text-sm text-yellow-800">• Database maintenance scheduled for tonight</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
