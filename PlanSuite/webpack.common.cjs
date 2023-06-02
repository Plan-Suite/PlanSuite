const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const directory = __dirname;

console.log("directory: " + directory);

module.exports = {
  entry: path.resolve(directory, './scripts/index.ts'),
  plugins: [
     new HtmlWebpackPlugin({
       title: 'Production',
     }),
   ],
  module: {
    rules: [
      {
        test: /\.ts?$/,
        use: ['ts-loader'],
        exclude: /node_modules/
      },
    ],
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js'],
  },
  output: {
    library: {
      name: 'PlanSuite',
      type: 'var'
    },
    filename: '[name].js',
    path: path.resolve(directory, './wwwroot/js'),
  }
};