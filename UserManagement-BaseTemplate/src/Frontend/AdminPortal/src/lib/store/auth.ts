import { create } from 'zustand';
import { User, LoginCredentials } from '@/types/auth';
import { apiService } from '@/lib/services/api';

interface AuthStore {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => void;
  setUser: (user: User) => void;
  loadUserFromStorage: () => void;
}

export const useAuthStore = create<AuthStore>((set, get) => ({
  user: null,
  token: null,
  isAuthenticated: false,

  setUser: (user: User) =>
    set({
      user,
      isAuthenticated: true,
    }),

  loadUserFromStorage: () => {
    if (typeof window !== 'undefined') {
      // Use setTimeout to avoid hydration issues
      setTimeout(() => {
        try {
          const token = localStorage.getItem('authToken');
          const userStr = localStorage.getItem('user');
          
          if (token && userStr) {
            try {
              const user = JSON.parse(userStr);
              set({
                user,
                token,
                isAuthenticated: true,
              });
            } catch (parseError) {
              // Clear invalid data
              localStorage.removeItem('authToken');
              localStorage.removeItem('user');
            }
          }
        } catch (error) {
          console.error('Error in loadUserFromStorage:', error);
        }
      }, 0);
    }
  },

  login: async (credentials: LoginCredentials) => {
    try {
      const response = await apiService.login(credentials);

      if (response.success) {
        // .NET API returns: { success, message, accessToken, user, ... }
        const { user: rawUser, accessToken } = response;
        
        // Process user data - add primary role for backward compatibility
        const user = {
          ...rawUser,
          role: rawUser.roles?.[0] || 'Member'  // Use first role as primary
        };
        
        // Store in localStorage
        if (typeof window !== 'undefined') {
          localStorage.setItem('authToken', accessToken);
          localStorage.setItem('user', JSON.stringify(user));
        }
        
        set({
          user,
          token: accessToken,
          isAuthenticated: true,
        });
      } else {
        throw new Error(response.message || 'Login failed');
      }
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  },

  logout: async () => {
    try {
      await apiService.logout();
    } catch (error) {
      // Ignore logout API errors
    }
    
    // Always clear local state
    if (typeof window !== 'undefined') {
      localStorage.removeItem('authToken');
      localStorage.removeItem('user');
    }
    
    set({
      user: null,
      token: null,
      isAuthenticated: false,
    });
  },
}));
