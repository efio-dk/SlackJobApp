# Region
provider "aws" {
  region = "eu-west-1"
}

# Data resources (SQS)
data "aws_sqs_queue" "prod-processed-job-post-queue" {
  name = "prod_ProcessedJobPosts"
}

# Lambda
resource "aws_lambda_function" "prod-SlackJobPoster-lambda" {
  function_name    = "prod-SlackJobPoster"
  handler          = "SlackJobPoster::SlackJobPoster.Function::FunctionHandler"
  runtime          = "dotnetcore2.1"
  role             = "arn:aws:iam::833191605868:role/SlackJobPosterRole"
  filename         = "../../src/SlackJobPoster/bin/Release/netcoreapp2.1/SlackJobPoster.zip"
  source_code_hash = filebase64sha256("../../src/SlackJobPoster/bin/Release/netcoreapp2.1/SlackJobPoster.zip")
  timeout          = 10

  tags = {
    Name        = "prod-SlackJobPoster"
    Environment = "production"
  }
}

resource "aws_lambda_event_source_mapping" "prod-incoming-sqs" {
  event_source_arn = data.aws_sqs_queue.prod-processed-job-post-queue.arn
  function_name    = aws_lambda_function.prod-SlackJobPoster-lambda.arn
}