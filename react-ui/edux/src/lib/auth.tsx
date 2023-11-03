import { configureAuth } from 'react-query-auth';

import { Spinner } from '@/components/Elements';

import {
  signIn,
  signUp,
  getUserMe,
  UserMeResponse,
  SignInRequest,
  SignUpRequest,
  JsonWebTokenResponse,
} from '@/features/users';

import storage from '@/utils/storage';

async function handleUserResponse(data: JsonWebTokenResponse) {
  storage.setToken(JSON.stringify(data));
  return data;
}

async function loadUser() {
  if (storage.getToken()) {
    const data = await getUserMe();
    return data;
  }
  return null;
}

async function login(data: SignInRequest) {
  const response = await signIn(data);
  const user = await handleUserResponse(response);
  return user;
}

async function register(data: SignUpRequest) {
  await signUp(data);
}

async function logout() {
  storage.clearToken();
  window.location.assign(window.location.origin as unknown as string);
}

const authConfig = {
  loadUser,
  loginFn,
  registerFn,
  logoutFn,
  LoaderComponent() {
    return (
      <div className="w-screen h-screen flex justify-center items-center">
        <Spinner size="xl" />
      </div>
    );
  }
};

export const { useUser, useLogin, useRegister, useLogout } = configureAuth(
    {
        userFn: loadUser,
        loginFn: login,
        registerFn: register,
        logoutFn: logout
    }
);