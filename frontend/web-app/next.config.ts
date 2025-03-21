import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  /* config options here */
  devIndicators : false,
  logging: {
    fetches: {
      fullUrl: true
    }
  }
};

export default nextConfig;
