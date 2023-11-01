import { axios } from '@/lib/axios';
import { SignUpResponse } from '../types';
import { SignUpRequest } from '../types';


export const signUp = (data: SignUpRequest): Promise<SignUpResponse> => {
    return axios.post(`/users-module/users/sign-up`, data);
};
