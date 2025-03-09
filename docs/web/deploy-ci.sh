BUCKET_NAME=($(aws ssm get-parameter --name "/$CONTEXTIVE_STAGE/docs/bucketName" --query Parameter.Value --output text))
SHA=($(git rev-parse --short HEAD))
VERSION=($(jq .version package.json -r))
aws s3 sync dist s3://$BUCKET_NAME/ide/$VERSION+$SHA