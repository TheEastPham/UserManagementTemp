import { apiClient } from './client';

export interface User {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  emailConfirmed: boolean;
  isActive: boolean;
  createdAt: string;
  lastLoginAt: string | null;
  roles: string[];
}

export interface UserFilterRequest {
  searchTerm?: string;
  role?: string;
  isActive?: boolean;
  page: number;
  pageSize: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AssignRoleRequest {
  userId: string;
  roleName: string;
}

export interface ApiResponse {
  success: boolean;
  message: string;
}

export const userManagementApi = {
  /**
   * Get users with pagination and filtering
   */
  async getUsers(filter: UserFilterRequest): Promise<PagedResult<User>> {
    const params = new URLSearchParams();
    
    if (filter.searchTerm) params.append('searchTerm', filter.searchTerm);
    if (filter.role) params.append('role', filter.role);
    if (filter.isActive !== undefined) params.append('isActive', filter.isActive.toString());
    params.append('page', filter.page.toString());
    params.append('pageSize', filter.pageSize.toString());

    const response = await apiClient.get<PagedResult<User>>(`/api/user?${params.toString()}`);
    return response.data;
  },

  /**
   * Get user by ID
   */
  async getUserById(userId: string): Promise<User> {
    const response = await apiClient.get<User>(`/api/user/${userId}`);
    return response.data;
  },

  /**
   * Ban/Block a user
   */
  async banUser(userId: string): Promise<ApiResponse> {
    const response = await apiClient.post<ApiResponse>(`/api/user/${userId}/ban`);
    return response.data;
  },

  /**
   * Unban/Unblock a user
   */
  async unbanUser(userId: string): Promise<ApiResponse> {
    const response = await apiClient.post<ApiResponse>(`/api/user/${userId}/unban`);
    return response.data;
  },

  /**
   * Assign role to user
   */
  async assignRole(userId: string, roleName: string): Promise<ApiResponse> {
    const request: AssignRoleRequest = { userId, roleName };
    const response = await apiClient.post<ApiResponse>(`/api/user/${userId}/roles`, request);
    return response.data;
  },

  /**
   * Remove role from user
   */
  async removeRole(userId: string, roleName: string): Promise<ApiResponse> {
    const response = await apiClient.delete<ApiResponse>(`/api/user/${userId}/roles/${roleName}`);
    return response.data;
  },

  /**
   * Get user roles
   */
  async getUserRoles(userId: string): Promise<string[]> {
    const response = await apiClient.get<string[]>(`/api/user/${userId}/roles`);
    return response.data;
  }
};
