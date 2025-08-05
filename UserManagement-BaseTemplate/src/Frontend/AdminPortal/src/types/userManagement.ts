export interface User {
  id: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  emailConfirmed?: boolean;
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

export interface UserRole {
  id: string;
  name: string;
  description?: string;
}
