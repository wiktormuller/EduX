import { Button } from '@/components/Elements';
import { Form, InputField } from '@/components/Form';
import { useAuth } from '@/lib/auth';
import * as z from 'zod';
import { Link } from 'react-router-dom';

const schema = z.object({
    email: z.string().min(1, 'Required'),
    password: z.string().min(1, 'Required')
});

type SignInValues = {
    email: string;
    password: string;
};

type SignInFormProps = {
    onSuccess: () => void;
};

export const SignInForm = ({ onSuccess }: SignInFormProps) => {
    const { login, isSigningIn } = useAuth;

    return (
        <div>
            <Form<SignInValues, typeof schema>
                onSubmit={async (values) => {
                    await login(values);
                    onSuccess();
                }}
                schema={schema}
            >
                {({ register, formState }) => (
                    <>
                        <InputField
                            type="email"
                            label="Email Address"
                            error={formState.errors['email']}
                            registration={register('email')}
                        />
                        <InputField
                            type="password"
                            label="Password"
                            error={formState.errors['password']}
                            registration={register('password')}
                        />
                        <div>
                            <Button isLoading={isSigningIn} type="submit" className="w-full">
                                SignIn
                            </Button>
                        </div>
                    </>
                )}
            </Form>
            <div className="mt-2 flex items-center justify-end">
                <div className="text-sm">
                    <Link to="../signup" className="font-medium text-blue-600 hover:text-blue-500">
                        SignUp
                    </Link>
                </div>
            </div>
        </div>
    );
};