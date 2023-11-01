import { axios } from '@/lib/axios';
import { JsonWebTokenResponse } from '../types';
import { SignInRequest } from '../types';

export const signIn = (data: SignInRequest): Promise<JsonWebTokenResponse> => {
    return axios.post(`/users-module/users/sign-in`, data);
};