import { AuthState } from '@/types/auth';
import { StateCreator } from 'zustand';

export type SetState<T> = (
  partial: T | Partial<T> | ((state: T) => T | Partial<T>),
  replace?: boolean
) => void;

export type AuthSlice = AuthState & {
  login: (credentials: { email: string; password: string }) => Promise<void>;
  logout: () => void;
};
