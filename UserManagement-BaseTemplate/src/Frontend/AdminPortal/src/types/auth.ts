export interface User {
  id: string;  // .NET returns string ID
  email: string;
  firstName?: string;
  lastName?: string;
  roles: string[];  // .NET returns roles array
  role?: string;   // Computed primary role for backward compatibility
}

export interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
}

export interface LoginCredentials {
  email: string;
  password: string;
}
