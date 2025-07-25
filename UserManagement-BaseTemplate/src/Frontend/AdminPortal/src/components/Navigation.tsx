'use client';

import React from 'react';
import { useAuthStore } from '@/lib/store/auth';

interface NavigationProps {
  currentPage?: string;
}

export default function Navigation({ currentPage }: NavigationProps): React.JSX.Element {
  const user = useAuthStore((state) => state.user);
  const logout = useAuthStore((state) => state.logout);

  const handleLogout = () => {
    logout();
    window.location.href = '/login';
  };

  const handleNavigation = (path: string) => {
    window.location.href = path;
  };

  return (
    <div className="bg-white shadow">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center py-6">
          <div className="flex items-center space-x-8">
            <h1 className="text-2xl font-bold text-gray-900">
              Admin Portal
            </h1>
            <nav className="flex space-x-6">
              <button
                onClick={() => handleNavigation('/dashboard')}
                className={`text-sm font-medium ${
                  currentPage === 'dashboard' 
                    ? 'text-indigo-600' 
                    : 'text-gray-500 hover:text-gray-900'
                }`}
              >
                Dashboard
              </button>
              <button
                onClick={() => handleNavigation('/profile')}
                className={`text-sm font-medium ${
                  currentPage === 'profile' 
                    ? 'text-indigo-600' 
                    : 'text-gray-500 hover:text-gray-900'
                }`}
              >
                Profile
              </button>
              <button
                className="text-sm font-medium text-gray-500 hover:text-gray-900"
              >
                Users
              </button>
              <button
                className="text-sm font-medium text-gray-500 hover:text-gray-900"
              >
                Settings
              </button>
            </nav>
          </div>
          <div className="flex items-center space-x-4">
            {user && (
              <div className="flex items-center space-x-3">
                <div className="h-8 w-8 rounded-full bg-indigo-600 flex items-center justify-center">
                  <span className="text-white font-medium text-sm">
                    {user.email[0].toUpperCase()}
                  </span>
                </div>
                <span className="text-sm text-gray-700">{user.email}</span>
              </div>
            )}
            <button
              onClick={handleLogout}
              className="bg-red-600 hover:bg-red-700 text-white font-bold py-2 px-4 rounded text-sm"
            >
              Logout
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
