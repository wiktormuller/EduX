import { axios } from '@/lib/axios';
import { JsonWebToken } from '../types';
import { LoginRequest } from '../types';

export const signIn = (data: LoginRequest): Promise<JsonWebToken> => {
    return axios.post(`/users-module/users/sign-in`, data);
};