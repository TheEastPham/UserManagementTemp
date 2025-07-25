import { NextResponse } from 'next/server';
import { headers } from 'next/headers';

// Rate limiting
const RATE_LIMIT_WINDOW = 60 * 1000; // 1 minute
const MAX_REQUESTS = 5;
const loginAttempts = new Map<string, { count: number; timestamp: number }>();

// Input validation without zod for now
const validateLoginInput = (data: any) => {
  if (!data.email || typeof data.email !== 'string' || !data.email.includes('@')) {
    return { isValid: false, error: 'Invalid email format' };
  }
  if (!data.password || typeof data.password !== 'string' || data.password.length < 8) {
    return { isValid: false, error: 'Password must be at least 8 characters' };
  }
  return { isValid: true };
};

export async function POST(request: Request) {
  try {
    // Get client IP for rate limiting
    const headersList = headers();
    const clientIP = headersList.get('x-forwarded-for') || 'unknown';
    
    // Check rate limit
    const now = Date.now();
    const userAttempts = loginAttempts.get(clientIP);
    
    if (userAttempts) {
      if (now - userAttempts.timestamp < RATE_LIMIT_WINDOW) {
        if (userAttempts.count >= MAX_REQUESTS) {
          return NextResponse.json(
            { success: false, message: 'Too many login attempts. Please try again later.' },
            { status: 429 }
          );
        }
        userAttempts.count++;
      } else {
        loginAttempts.set(clientIP, { count: 1, timestamp: now });
      }
    } else {
      loginAttempts.set(clientIP, { count: 1, timestamp: now });
    }

    // Validate CSRF token (temporarily disabled for testing)
    // const csrfToken = headersList.get('x-csrf-token');
    // if (!csrfToken) {
    //   return NextResponse.json(
    //     { success: false, message: 'Invalid request' },
    //     { status: 403 }
    //   );
    // }

    const body = await request.json();
    
    // Validate input
    const validationResult = validateLoginInput(body);
    if (!validationResult.isValid) {
      return NextResponse.json(
        { success: false, message: validationResult.error },
        { status: 400 }
      );
    }

    const { email, password } = body;

    // TODO: Replace with actual authentication logic
    // This is where you'll integrate with your backend authentication service
    if (email === 'admin@example.com' && password === 'admin123') {
      // Clear rate limiting for successful login
      loginAttempts.delete(clientIP);
      
      // In a real app, you would:
      // 1. Validate credentials against your backend
      // 2. Generate a JWT token with proper expiration
      // 3. Set secure HTTP-only cookies
      const response = NextResponse.json({
        success: true,
        user: {
          id: 1,
          email,
          role: 'admin'
        }
      });

      // Set secure cookie with JWT token
      response.cookies.set({
        name: 'authToken',
        value: 'dummy-jwt-token',
        httpOnly: true,
        secure: false, // Set to true in production
        sameSite: 'strict',
        maxAge: 60 * 60 * 24 // 24 hours
      });

      return response;
    }

    // Increment failed attempts counter
    const attempts = loginAttempts.get(clientIP);
    if (attempts) {
      attempts.count++;
    }

    return NextResponse.json(
      { success: false, message: 'Invalid credentials' },
      { status: 401 }
    );
  } catch (error) {
    console.error('Login error:', error);
    return NextResponse.json(
      { success: false, message: 'Internal server error' },
      { status: 500 }
    );
  }
}
