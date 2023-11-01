import { axios } from '@/lib/axios';
import { UserMeResponse } from '../types';

export const getUserMe = (): Promise<UserMeResponse> => {
  return axios.get('/auth/me');
};