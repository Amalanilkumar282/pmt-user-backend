-- Create refresh_tokens table for JWT token management
CREATE TABLE IF NOT EXISTS public.refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id INTEGER NOT NULL,
    token VARCHAR(500) NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    revoked_at TIMESTAMP WITH TIME ZONE,
    replaced_by_token VARCHAR(500),
    CONSTRAINT fk_refresh_tokens_user FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX idx_refresh_tokens_user_id ON public.refresh_tokens(user_id);
CREATE INDEX idx_refresh_tokens_token ON public.refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_expires_at ON public.refresh_tokens(expires_at);

-- Add comments
COMMENT ON TABLE public.refresh_tokens IS 'Stores refresh tokens for JWT authentication';
COMMENT ON COLUMN public.refresh_tokens.user_id IS 'Reference to the user who owns this token';
COMMENT ON COLUMN public.refresh_tokens.token IS 'The refresh token string';
COMMENT ON COLUMN public.refresh_tokens.expires_at IS 'When the token expires';
COMMENT ON COLUMN public.refresh_tokens.revoked_at IS 'When the token was revoked (if revoked)';
COMMENT ON COLUMN public.refresh_tokens.replaced_by_token IS 'New token that replaced this one';
