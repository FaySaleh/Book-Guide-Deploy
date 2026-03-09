export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface UserDto {
  id: number;
  fullName: string;
  email: string;
  createdAt?: string;
}
