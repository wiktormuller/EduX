export type JsonWebTokenResponse = {
    id: string
    accessToken: string;
    refreshToken: string;
    expires: number,
    role: string,
    email: string
    claims: Map<string, string[]>
};

export type SignUpResponse = {
};

export type UserMeResponse = {
    id: string,
    email: string,
    role: string,
    isActive: boolean,
    createdAt: Date,
    updatedAt: Date,
    claims: Map<string, string[]>
};

export type SignInRequest = {
    email: string,
    password: string
};

export type SignUpRequest = {
    email: string,
    password: string
};