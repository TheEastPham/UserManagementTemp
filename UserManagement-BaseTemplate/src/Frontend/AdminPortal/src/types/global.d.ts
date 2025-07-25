/// <reference types="react" />

declare module "@/lib/store/auth" {
  import { AuthSlice } from "@/lib/types/store";
  
  export const useAuthStore: import("zustand").UseBoundStore<
    import("zustand").StoreApi<AuthSlice>
  >;
}

declare module "@/types/auth" {
  export interface LoginCredentials {
    email: string;
    password: string;
  }

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
}
