/**
 * API Service Configuration
 * 
 * Architecture:
 * Frontend (:3000) → API Gateway (:5000) → UserManagement API (:5001)
 * 
 * The frontend communicates with the API Gateway (Ocelot) which routes
 * requests to the appropriate backend services.
 * 
 * Routes configured in Gateway:
 * - /api/users/* → UserManagement API
 * - /api/auth/* → UserManagement API  
 * - /api/account/* → UserManagement API
 * - /api/limitation/* → UserManagement API
 * 
 * Local Development: http://localhost:5000/api
 * Docker Environment: http://api-gateway:8080/api (handled by environment variable)
 */

// API Base Configuration - Updated to use API Gateway
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

class ApiService {
  private baseURL: string;

  constructor(baseURL: string = API_BASE_URL) {
    this.baseURL = baseURL;
  }

  private getAuthToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('authToken');
    }
    return null;
  }

  private async makeRequest<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    try {
      const url = `${this.baseURL}${endpoint}`;
      const defaultHeaders: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      // Get auth token from localStorage
      const token = this.getAuthToken();
      
      if (token) {
        defaultHeaders['Authorization'] = `Bearer ${token}`;
      }

      const response = await fetch(url, {
        ...options,
        mode: 'cors',
        credentials: 'omit',
        headers: {
          ...defaultHeaders,
          ...options.headers,
        },
      });

      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || 'API request failed');
      }

      return data;
    } catch (error) {
      console.error(`API Error for ${endpoint}:`, error);
      throw error;
    }
  }

  // Auth API Methods
  async login(credentials: { email: string; password: string }) {
    return this.makeRequest<{
      success: boolean;
      message: string;
      accessToken: string;
      refreshToken?: string;
      expiresAt?: string;
      user: { 
        id: string; 
        email: string; 
        firstName?: string;
        lastName?: string;
        roles: string[];  // .NET returns roles as array
        role?: string;    // For backward compatibility
      };
    }>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(credentials),
    });
  }

  async logout() {
    return this.makeRequest<void>('/auth/logout', {
      method: 'POST',
    });
  }

  async refreshToken() {
    return this.makeRequest<{ token: string }>('/auth/refresh', {
      method: 'POST',
    });
  }

  // User Profile API Methods
  async getCurrentUser() {
    return this.makeRequest<{
      id: string;
      email: string;
      firstName: string;
      lastName: string;
      roles: string[];
      emailConfirmed: boolean;
      isActive: boolean;
      createdAt: string;
      lastLoginAt?: string;
    }>('/account/profile');
  }

  async updateProfile(profileData: {
    firstName: string;
    lastName: string;
    email: string;
    department: string;
    phone: string;
  }) {
    // Map frontend fields to backend DTO format
    const updateRequest = {
      firstName: profileData.firstName,
      lastName: profileData.lastName,
      phoneNumber: profileData.phone,
      // Note: email and department updates handled separately or ignored for security
    };
    
    return this.makeRequest<void>('/account/profile', {
      method: 'PUT',
      body: JSON.stringify(updateRequest),
    });
  }

  async changePassword(passwordData: {
    currentPassword: string;
    newPassword: string;
  }) {
    return this.makeRequest<void>('/account/password', {
      method: 'PUT',
      body: JSON.stringify(passwordData),
    });
  }

  async getUserActivity() {
    return this.makeRequest<Array<{
      id: number;
      action: string;
      timestamp: string;
      details: string;
    }>>('/users/me/activity');
  }

  // Admin User Management API Methods (SystemAdmin only)
  async getAllUsers(params?: {
    page?: number;
    pageSize?: number;
    role?: string;
    searchTerm?: string;
  }) {
    const searchParams = new URLSearchParams();
    if (params?.page) searchParams.append('page', params.page.toString());
    if (params?.pageSize) searchParams.append('pageSize', params.pageSize.toString());
    if (params?.role) searchParams.append('role', params.role);
    if (params?.searchTerm) searchParams.append('searchTerm', params.searchTerm);
    
    return this.makeRequest<{
      users: Array<{
        id: string;
        email: string;
        firstName: string;
        lastName: string;
        fullName: string;
        isActive: boolean;
        roles: string[];
        createdAt: string;
        lastLoginAt?: string;
      }>;
      totalCount: number;
      page: number;
      pageSize: number;
      totalPages: number;
    }>(`/users?${searchParams.toString()}`);
  }

  async getUserById(id: string) {
    return this.makeRequest<{
      id: string;
      email: string;
      firstName: string;
      lastName: string;
      fullName: string;
      dateOfBirth?: string;
      avatar?: string;
      timeZone?: string;
      language?: string;
      createdAt: string;
      updatedAt: string;
      isActive: boolean;
      lastLoginAt?: string;
      roles: string[];
    }>(`/users/${id}`);
  }

  async createUser(userData: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    dateOfBirth?: string;
    timeZone?: string;
    language?: string;
  }) {
    return this.makeRequest<{
      id: string;
      email: string;
      firstName: string;
      lastName: string;
      fullName: string;
      isActive: boolean;
      roles: string[];
      createdAt: string;
    }>('/users', {
      method: 'POST',
      body: JSON.stringify(userData),
    });
  }

  async updateUser(id: string, userData: {
    firstName: string;
    lastName: string;
    dateOfBirth?: string;
    avatar?: string;
    timeZone?: string;
    language?: string;
    phoneNumber?: string;
    email?: string;
  }) {
    return this.makeRequest<{
      id: string;
      email: string;
      firstName: string;
      lastName: string;
      fullName: string;
      isActive: boolean;
      roles: string[];
      updatedAt: string;
    }>(`/users/${id}`, {
      method: 'PUT',
      body: JSON.stringify({
        id,
        ...userData,
      }),
    });
  }

  async deleteUser(id: string) {
    return this.makeRequest<void>(`/users/${id}`, {
      method: 'DELETE',
    });
  }

  async assignRole(userId: string, roleName: string) {
    return this.makeRequest<void>(`/users/${userId}/roles`, {
      method: 'POST',
      body: JSON.stringify({
        userId,
        roleName,
      }),
    });
  }

  async removeRole(userId: string, roleName: string) {
    return this.makeRequest<void>(`/users/${userId}/roles/${roleName}`, {
      method: 'DELETE',
    });
  }

  // System API Methods
  async getSystemStats() {
    return this.makeRequest<{
      totalUsers: number;
      activeUsers: number;
      systemStatus: string;
      lastBackup: string;
    }>('/system/stats');
  }
}

export const apiService = new ApiService();
